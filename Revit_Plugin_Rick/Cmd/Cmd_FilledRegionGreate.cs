using System;
using System.Collections.Generic;
using System.Linq;
using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.Utils;
using Revit_Plugin_Rick.Utils.CurveUtils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;



/// <summary>
/// create filled region
/// </summary>
namespace Revit_Plugin_Rick
{
    [Transaction(TransactionMode.Manual)]
    class Cmd_FilledRegionGreate : RevitCommand
    {
        RevitTask task;
        
        
        FilledRegionCreateUpdater updater;

        public override Result Action()
        {
            //get basic info
            task = new RevitTask();
            //CreateNewLineStyle();
           

            //init window
            var window = new FilledRegionCreateWindow(new FilledRegionCreateWindow.UnRegister(UnRegister));
            window.MainWindowHandle();
            window.Show();

            //register updater
            updater = new FilledRegionCreateUpdater(RevitDoc.Instance.App.ActiveAddInId);
            updater.RegisterUpdater();

            var detailLineCommandId = RevitCommandId.LookupCommandId("ID_OBJECTS_DETAIL_CURVES");
            
           
            RevitDoc.Instance.UIapp.PostCommand(detailLineCommandId);

            return Result.Succeeded;
        }


        public void UnRegister()
        {
            task.Run(x =>
            {
                updater.UnregisterUpdater();
            });
        }


    }

    
}
