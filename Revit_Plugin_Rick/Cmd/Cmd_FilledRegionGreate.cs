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
        List<ElementId> addedCurveIds = new List<ElementId>();
        Category newLineCat = null;

        public override Result Action()
        {
            task = new RevitTask();
            CreateNewLineStyle();
            var window = new FilledRegionCreateWindow(new FilledRegionCreateWindow.UnRegister(UnRegister));
            window.MainWindowHandle();
            window.Show();


            var detailLineCommandId = RevitCommandId.LookupCommandId("ID_OBJECTS_DETAIL_CURVES");
            Register();
           
            RevitDoc.Instance.UIapp.PostCommand(detailLineCommandId);

            //RevitDoc.Instance.App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
            return Result.Succeeded;
        }

        

        public void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            addedCurveIds.AddRange(e.GetAddedElementIds());

            foreach(ElementId id in e.GetAddedElementIds())
            {
                DetailCurve curve = RevitDoc.Instance.Doc.GetElement(id) as DetailCurve;
                if (curve != null)
                {
                    addedCurveIds.Add(curve.Id);
                    /*Transaction trans = new Transaction(RevitDoc.Instance.Doc, "change line style");
                    trans.Start();
                    curve.LineStyle = newLineCat.GetGraphicsStyle(GraphicsStyleType.Projection);
                    trans.Commit();*/
                }
                
            }
            
            //RevitDoc.Instance.App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        }

        public void Register()
        {
            RevitDoc.Instance.App.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
        }

        public void UnRegister()
        {
            
            task.Run(x =>
            {
                using (Transaction trans = new Transaction(RevitDoc.Instance.Doc, "UnRegister"))
                {
                    trans.Start();
                    RevitDoc.Instance.App.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
                    trans.Commit();
                }
            });
            
            
        }

        public void CreateNewLineStyle()
        {
            
            using(var ts = new Transaction(RevitDoc.Instance.Doc,"create new line style"))
            {
                ts.Start();
                Category lineC = RevitDoc.Instance.Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
                var subC = lineC.SubCategories;
                foreach(Category item in subC)
                {
                    if(item.Name == "填充边缘线")
                    {
                        newLineCat = item;
                    }
                }
                if(newLineCat == null)
                    newLineCat = RevitDoc.Instance.Doc.Settings.Categories.NewSubcategory(lineC, "填充边缘线");
                
                newLineCat.LineColor = new Color(250, 10, 10);
                newLineCat.SetLineWeight(10, GraphicsStyleType.Projection);
                ts.Commit();
            }
        }



    }

    
}
