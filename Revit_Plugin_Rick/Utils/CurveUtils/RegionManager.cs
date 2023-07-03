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

namespace Revit_Plugin_Rick.Utils.CurveUtils
{
    public class RegionManager
    {
        private List<RegionParser> parsers;
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
                p.PerfectGroup();
            }
        }
    }
}
