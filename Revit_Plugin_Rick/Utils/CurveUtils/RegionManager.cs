using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;
using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.Utils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Collections;
using ClipperLib;
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace Revit_Plugin_Rick.Utils.CurveUtils
{
    public class RegionManager
    {
        public delegate void ClosingEvent(List<Curve[]> cs);
        public ClosingEvent closingEvent;

        CoordSpaceConverter coordConverter;
        bool perfected = false;
        float scaleFac = 1000000;
        public RegionManager()
        {
            coordConverter = new CoordSpaceConverter(RevitDoc.Instance.UIdoc);
        }

        private List<RegionParser> parsers = new List<RegionParser>();
        public List<RegionParser> Parsers
        {
            get
            {
                return parsers;
            }
        }

        /// <summary>
        /// this func will initiate a new RegionParser and parse input curve list
        /// </summary>
        /// <param name="curves"></param>
        public void ParseNewCurveList(List<Curve> curves)
        {
            RegionParser parser = new RegionParser();
            foreach(Curve c in curves)
            {
                parser.AddCurve(c);
            }
            parsers.Add(parser);
        }

        /// <summary>
        /// this func will use latest regionParser to parse input curve list
        /// </summary>
        /// <param name="curves"></param>
        public void ParseCurveList(List<Curve> curves)
        {
            foreach(Curve c in curves)
            {
                parsers[parsers.Count - 1].AddCurve(c);
            }
        }

        /// <summary>
        /// always use same parser to parse add region curve
        /// </summary>
        /// <param name="curve"></param>
        public void ParseAddRegionCurve(Curve curve)
        {
            if (parsers.Count == 0)
            {
                var parser = new RegionParser();
                RegionParser.closing = new RegionParser.Closing(DetectedClosing);
                parsers.Add(parser);
                
            }
            
            parsers[0].AddCurve(curve);
        }

        public void ParseDeleteRegionCurve(Curve curve)
        {
            if(parsers.Count < 2)
            {
                for(int i = 0; i < 2 - parsers.Count; i++)
                {
                    parsers.Add(new RegionParser());
                }
            }
        }

        public void DetectedClosing()
        {
            var curvesLists = GetUnionRegionCurve(0);
            if(closingEvent != null)
            {
                closingEvent(curvesLists);
            }
        }


        /// <summary>
        /// Rearrange curves,make curves close and clockwise in all region parsers,this function must be called after all curve parsing has done
        /// </summary>
        public void PerfectParsers()
        {
            foreach(var p in parsers)
            {
                p.RearrangeFloatCurves();
                p.ExtendToSnapFloatCurvesAndGroups();
                
                p.PerfectGroup();
            }
            perfected = true;
        }

        public IEnumerable<Curve[]> GetClosedCurves(int ind = -1)
        {
            if(ind != -1)
            {
                foreach(Curve[] crs in parsers[ind].GetClosedCurves())
                {
                    yield return crs;
                }
            }
            else
            {
                foreach(var p in parsers)
                {
                    foreach(Curve[] crs in p.GetClosedCurves())
                    {
                        yield return crs;
                    }
                }
            }
            yield break;
        }

        


        /// <summary>
        /// Return unioned region boundary, 
        /// WARNING!: Returned boundary unioned in screen space, not the same with original curves
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public List<Curve[]> GetUnionRegionCurve(int ind = -1)
        {
            if (!perfected && ind == -1) return null;
            Paths ps = new Paths();
            
            //get all closed path
            if(ind != -1)
            {
                foreach(var cs in GetClosedCurves(ind))
                {
                    Path path = new Path();
                    foreach (var c in cs)
                    {
                        XYZ screen_head = coordConverter.Model2Screen(c.GetEndPoint(0));
                        XYZ screen_tail = coordConverter.Model2Screen(c.GetEndPoint(1));
                        path.Add(new IntPoint(screen_head.X * scaleFac, screen_head.Y * scaleFac));

                    }
                    //csList_screen.Add(cs);
                    ps.Add(path);
                }
            }
            else
            {
                foreach(var cs in GetClosedCurves())
                {
                    Path path = new Path();
                    foreach (var c in cs)
                    {
                        XYZ screen_head = coordConverter.Model2Screen(c.GetEndPoint(0));
                        XYZ screen_tail = coordConverter.Model2Screen(c.GetEndPoint(1));
                        path.Add(new IntPoint(screen_head.X*scaleFac, screen_head.Y*scaleFac));
                    
                    }
                    //csList_screen.Add(cs);
                    ps.Add(path);
                }
            }

            //union
            for(int i = 0; i < ps.Count; i++)
            {
                //Paths subj = new Paths(1);
                //subj.Add(ps[i].points);
                for (int j = i+1; j < ps.Count; j++)
                {
                    //Paths clip = new Paths(1);
                    //clip.Add(ps[j].points);

                    Clipper c = new Clipper();
                    
                    c.AddPath(ps[i], PolyType.ptSubject, true);
                    c.AddPath(ps[j], PolyType.ptClip, true);
                    Paths solution = new Paths();
                    if (c.Execute(ClipType.ctUnion, solution))
                    {
                        if (solution.Count == 2) continue;
                        ps[i]= solution[0];
                        //i--;
                        ps.RemoveAt(j);
                    }
                }
            }

            //back to revit curve
            List<Curve[]> results = new List<Curve[]>();
            foreach(var path in ps)
            {
                List<Curve> curves = new List<Curve>();
                for(int i = 0; i < path.Count; i++)
                {
                    XYZ screenHead = new XYZ(path[i].X/scaleFac, path[i].Y/scaleFac, 0);
                    XYZ screenTail = new XYZ(path[(i + 1) % path.Count].X/scaleFac, path[(i + 1) % path.Count].Y/scaleFac, 0);
                    XYZ modelHead = coordConverter.Screen2Model(screenHead);
                    XYZ modelTail = coordConverter.Screen2Model(screenTail);
                    Line l = Line.CreateBound(modelHead, modelTail);
                    curves.Add(l);
                }
                results.Add(curves.ToArray());
            }

            return results;
        }

        


    }
}
