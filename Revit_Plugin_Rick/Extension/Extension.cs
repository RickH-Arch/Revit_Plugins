using Autodesk.Revit.UI;
using Revit_Plugin_Rick.Utils;
using System.Windows;
using System.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;

namespace Revit_Plugin_Rick.Extension
{
    public static class Extension
    {
        public static UIDocument Init(this ExternalCommandData commandData)
        {
            RevitDoc.Instance.UIdoc = commandData.Application.ActiveUIDocument;
            RevitDoc.Instance.UIapp = commandData.Application;
            RevitDoc.Instance.App = commandData.Application.Application;
            RevitDoc.Instance.Doc = commandData.Application.ActiveUIDocument.Document;
            RevitDoc.Instance.Task = new RevitTask();
            return RevitDoc.Instance.UIdoc;
        }

        public static void MainWindowHandle(this Window window)
        {
            new WindowInteropHelper(window).Owner = RevitDoc.Instance.UIdoc.Application.MainWindowHandle;
        }
        public static Curve Reverse(this Curve curve)
        {
            if (curve is Line)
            {
                Line l = curve as Line;
                return l.CreateReversed();
            }
            if (curve is Arc)
            {
                Arc arc = curve as Arc;
                return arc.CreateReversed();
            }
            if (curve is NurbSpline)
            {
                NurbSpline ns = curve as NurbSpline;
                return ns.CreateReversed();
            }
            return null;
        }
    }
}
