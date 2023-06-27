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

namespace Revit_Plugin_Rick
{
    [Transaction(TransactionMode.Manual)]
    class Cmd_RegionFill : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            commandData.Init();
            var window = new RegionFillWindow();
            window.Show();

            return Result.Succeeded;
        }
    }
}
