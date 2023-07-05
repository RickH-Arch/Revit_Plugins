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
        /// this fun will use latest regionParser to parse input curve list
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
        /// make curves close and clockwise in all region parsers
        /// </summary>
        public void PerfectParsers()
        {
            foreach(var p in parsers)
            {
                p.ExpandToSnapFloatCurvesAndGroups();
                p.PerfectGroup();
            }
        }

        public IEnumerable<Curve[]> GetClosedCurves()
        {
            foreach(var p in parsers)
            {
                foreach(Curve[] crs in p.GetClosedCurves())
                {
                    yield return crs;
                }
            }
            yield break;
        }


        /// <summary>
        /// points in revit is 3D points, must give them a reference plane to reduce dimention
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        /*public Curve[] GetUnionRegionCurve(Plane refPlane)
        {

            Paths subjs = new Paths(3);

            
        }*/

        class Path
        {
            public List<IntPoint> points = new List<IntPoint>();
        }


    }
}
