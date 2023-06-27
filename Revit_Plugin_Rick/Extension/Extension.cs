using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Revit_Plugin_Rick.Utils;

namespace Revit_Plugin_Rick.Extension
{
    public static class Extension
    {
        public static UIDocument Init(this ExternalCommandData commandData)
        {
            XmlDoc.Instance.UIdoc = commandData.Application.ActiveUIDocument;
            XmlDoc.Instance.Task = new RevitTask();
            return XmlDoc.Instance.UIdoc;
        }
    }
}
