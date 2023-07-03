using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Plugin_Rick.Extension;

namespace Revit_Plugin_Rick.Utils
{
    internal abstract class RevitCommand : IExternalCommand
    {
        /// <summary>
        /// 具体功能逻辑实现
        /// </summary>
        public abstract Result Action();

        public UIDocument Uidoc { get; private set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Uidoc = commandData.Init();
                
                return Action();
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch(System.Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
