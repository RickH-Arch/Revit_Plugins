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


/// <summary>
/// create filled region
/// </summary>
namespace Revit_Plugin_Rick
{
    [Transaction(TransactionMode.Manual)]
    class Cmd_FilledRegionGreate : RevitCommand
    {
        public override Result Action()
        {
            //MouseHook

            var window = new FilledRegionCreateWindow();
            window.MainWindowHandle();
            window.Show();


            return Result.Succeeded;
        }
    }

    public class FillPatternPickFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem is FillPatternElement;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
