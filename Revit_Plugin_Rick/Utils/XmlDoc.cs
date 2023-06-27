using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Plugin_Rick.Utils
{
    internal class XmlDoc
    {
        private XmlDoc() { }
        private static readonly XmlDoc Global = new XmlDoc();
        public static XmlDoc Instance => Global;

        public UIDocument UIdoc { get; set; }

        public RevitTask Task { get; set; }
    }
}
