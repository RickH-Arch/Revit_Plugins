using System;
using System.Text;
using System.Linq;

using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using Revit_Plugin_Rick.Utils.CurveUtils;

namespace Revit_Plugin_Rick.Utils
{
    class FilledRegionCreateUpdater : IUpdater
    {
        AddInId addinID = null;
        UpdaterId updaterID = null;
        List<DetailCurve> addRegionCurves = new List<DetailCurve>();
        Category newLineCat;
        View activeView;
        Element fillType;
        RegionManager regionMgr;
        FilledRegion region = null;

        public FilledRegionCreateUpdater(AddInId id)
        {
            addinID = id;
            updaterID = new UpdaterId(addinID, new Guid(
                "63CDBB88-5CC4-4ac3-AD24-52DD435AAB25"));
            GetBasicInfo();
            CreateNewLineStyle();
            regionMgr = new RegionManager();
            regionMgr.closingEvent = new RegionManager.ClosingEvent(GenerateFilledRegion);
        }

        public void Execute(UpdaterData data)
        {
            var addedIds = data.GetAddedElementIds();
            foreach(ElementId id in addedIds)
            {
                DetailCurve c = RevitDoc.Instance.Doc.GetElement(id) as DetailCurve;
                if(c != null)
                {
                    //addRegionCurves.Add(c);
                    AddAddRegionCurve(c);
                    c.LineStyle = newLineCat.GetGraphicsStyle(GraphicsStyleType.Projection);
                    
                }
            }
        }

        private void AddAddRegionCurve(DetailCurve c)
        {
            regionMgr.ParseAddRegionCurve(c.GeometryCurve);
        }

        private void AddDeleteRegionCurve(DetailCurve c)
        {
            regionMgr.ParseDeleteRegionCurve(c.GeometryCurve);
        }

        private void GenerateFilledRegion(List<Curve[]> css)
        {
            if(region != null)
            {
                RevitDoc.Instance.Doc.Delete(region.Id);
            }

            IList<CurveLoop> bounds = new List<CurveLoop>();
            foreach(var cs in css)
            {
                CurveLoop cLoop = CurveLoop.Create(cs);
                bounds.Add(cLoop);
            }

            region = FilledRegion.Create(RevitDoc.Instance.Doc, fillType.Id, activeView.Id, bounds);


        }



        //info prepare
        /// ////////////////////////////////////////////////////////////////////////////
        
        private void CreateNewLineStyle()
        {
            using (var ts = new Transaction(RevitDoc.Instance.Doc, "create new line style"))
            {
                ts.Start();
                Category lineC = RevitDoc.Instance.Doc.Settings.Categories.get_Item(BuiltInCategory.OST_Lines);
                var subC = lineC.SubCategories;
                foreach (Category item in subC)
                {
                    if (item.Name == "填充边缘线")
                    {
                        newLineCat = item;
                    }
                }
                if (newLineCat == null)
                    newLineCat = RevitDoc.Instance.Doc.Settings.Categories.NewSubcategory(lineC, "填充边缘线");

                newLineCat.LineColor = new Color(250, 10, 10);
                newLineCat.SetLineWeight(10, GraphicsStyleType.Projection);
                ts.Commit();
            }
        }

        private void GetBasicInfo()
        {
            activeView = RevitDoc.Instance.UIdoc.ActiveView;
            var fillTypes = new FilteredElementCollector(RevitDoc.Instance.Doc)
                  .OfClass(typeof(FilledRegionType));
            fillType = fillTypes.FirstOrDefault(x => x.Name == "实体填充 - 黑色");
        }

        public void RegisterUpdater()
        {
            FilledRegionCreateUpdater updater = new FilledRegionCreateUpdater(RevitDoc.Instance.App.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);
            //trigger will occur only for detail curves
            ElementClassFilter filter = new ElementClassFilter(typeof(CurveElement));
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeElementAddition());
        }

        public void UnregisterUpdater()
        {
            FilledRegionCreateUpdater updater = new FilledRegionCreateUpdater(RevitDoc.Instance.App.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());
        }

        public string GetAdditionalInformation()
        {
            return "real time update filled region boundaried by detail curve";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterID;
        }

        public string GetUpdaterName()
        {
            return "Filled Region Create Updater";
        }
    }
}
