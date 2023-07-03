using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Plugin_Rick.Utils
{
    internal class RevitDoc
    {
        private RevitDoc() { }
        private static readonly RevitDoc Global = new RevitDoc();
        public static RevitDoc Instance => Global;

        public UIDocument UIdoc { get; set; }

        public UIApplication UIapp { get; set; }

        public Application App { get; set; }

        public Document Doc { get; set; }

        public RevitTask Task { get; set; }
    }
}
