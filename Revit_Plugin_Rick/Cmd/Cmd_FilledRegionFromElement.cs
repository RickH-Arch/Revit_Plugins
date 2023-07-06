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
using Revit_Plugin_Rick.Utils.CurveUtils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;

namespace Revit_Plugin_Rick
{
    /// <summary>
    /// create filledregion from element section
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    class Cmd_FilledRegionFromElement : RevitCommand
    {
        //List<Reference> selectedRefs = new List<Reference>();
        Plane plane;
        Options opt;
        View clipView;
        

        //public List<RegionParser> regionParsers = new List<RegionParser>();
        RegionManager regionMgr = new RegionManager();
        
        /// <summary>
        /// Maximum distance for line to be 
        /// considered to lie in plane
        /// </summary>
        const double LineOnPlane_THRESHOLD = 1.0e-6;

        

        public override Result Action()
        {
            //judge if active view is right
            View activeView = RevitDoc.Instance.UIdoc.ActiveView;
            View3D viewNow = activeView as View3D;
            

            if(activeView.ViewType != ViewType.Section 
                && activeView.ViewType != ViewType.FloorPlan
                && activeView.ViewType != ViewType.CeilingPlan
                )
            {
                TaskDialog.Show("Error", "当前视图不满足创建条件");
                return Result.Cancelled;
            }

            using (Transaction tr = new Transaction(RevitDoc.Instance.Doc,"初始化剖切面"))
            {
                tr.Start();
                
                if(activeView is ViewPlan)
                {
                    var view = activeView as ViewPlan;
                    PlanViewRange viewRange = view.GetViewRange();
                    double cutPlaneHeight = viewRange.GetOffset(PlanViewPlane.CutPlane);
                    clipView = RevitDoc.Instance.Doc.GetElement(activeView.Duplicate(ViewDuplicateOption.Duplicate)) as ViewPlan;
                    clipView.DisplayStyle = DisplayStyle.HLR;
                    var clipParam = clipView.get_Parameter(BuiltInParameter.VIEW_BACK_CLIPPING);
                    clipParam.Set(1);//switch clip mode(0:no clip,1:clip with line,2:clip without line)
                    plane = Plane.CreateByNormalAndOrigin(clipView.ViewDirection,new XYZ( clipView.Origin.X,clipView.Origin.Y,clipView.Origin.Z+cutPlaneHeight));
                }
                else
                {
                    clipView = RevitDoc.Instance.Doc.GetElement(activeView.Duplicate(ViewDuplicateOption.Duplicate)) as View;
                    var clipParam = clipView.get_Parameter(BuiltInParameter.VIEWER_BOUND_ACTIVE_NEAR);
                    clipParam.Set(1);
                    plane = Plane.CreateByNormalAndOrigin(clipView.ViewDirection, clipView.Origin);
                }

                clipView.DetailLevel = ViewDetailLevel.Fine;
                opt = new Options() { ComputeReferences = false, IncludeNonVisibleObjects = false, View = clipView };


                tr.Commit();
            }

            /////////////////////////////////////////////////////////////////////////////
            //select item

            List<BuiltInCategory> categories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_Stairs,
                BuiltInCategory.OST_Floors,
            };

            ICollection<ElementId> selectedElemIds = RevitDoc.Instance.UIdoc.Selection.GetElementIds();
            List<ElementId> elemIds = new List<ElementId>();
            foreach(var id in selectedElemIds)
            {
                Element elem = RevitDoc.Instance.Doc.GetElement(id);
                foreach(var ca in categories)
                {
                    if(elem.Category.Id.IntegerValue == (int)ca)
                    {
                        elemIds.Add(id);
                    }
                }
            }
            if(elemIds.Count == 0)
            {
                var selectedRefs = RevitDoc.Instance.UIdoc.Selection.PickObjects(ObjectType.Element, new SectionElementSelecion(), "请选择要填充的元素").ToList();
                if (selectedRefs.Count == 0) return Result.Failed;
                foreach(var eRef in selectedRefs)
                {
                    var id = RevitDoc.Instance.Doc.GetElement(eRef).Id;
                    elemIds.Add(id);
                }
            }

            



            /////////////////////////////////////////////////////////////////////////////
            var fillTypes = new FilteredElementCollector(RevitDoc.Instance.Doc)
                .OfClass(typeof(FilledRegionType));
                
            var fillType = fillTypes.FirstOrDefault(x => x.Name == "实体填充 - 黑色");


            foreach (var re in elemIds)
            {
                List<Curve> curves = new List<Curve>();
                Element elem = RevitDoc.Instance.Doc.GetElement(re);
                GeometryElement geo = elem.get_Geometry(opt);
                GetCurvesInPlane(curves,plane, geo);
                regionMgr.ParseNewCurveList(curves);
            }

            regionMgr.PerfectParsers();
                
            
            using (Transaction tx = new Transaction(RevitDoc.Instance.Doc))
            {
                tx.Start("创建截面Curve");

                SketchPlane sketchP = SketchPlane.Create(
                  RevitDoc.Instance.Doc, plane);

                IList<CurveLoop> bounds = new List<CurveLoop>();
                foreach (Curve[] cs in regionMgr.GetClosedCurves())
                {
                    /*ModelCurveArray mca = new ModelCurveArray();
                    foreach(Curve c in cs)
                    {
                        var mc = RevitDoc.Instance.Doc.Create.NewModelCurve(c, sketchP);
                        mca.Append(mc);
                    }
                    DetailCurveArray dca = RevitDoc.Instance.Doc.ConvertModelToDetailCurves(activeView,mca);
                    foreach(var dc in dca)
                    {
                        var cc = dc as DetailCurve;
                        XYZ start = cc.GeometryCurve.GetEndPoint(0);
                        XYZ end = cc.GeometryCurve.GetEndPoint(1);
                    }*/
                    //CurveLoop cLoop = CurveLoop.Create(cs);
                    //bounds.Add(cLoop);
                }

                var curveLists = regionMgr.GetUnionRegionCurve();
                foreach(var cs in curveLists)
                {
                    CurveLoop cLoop = CurveLoop.Create(cs);
                    bounds.Add(cLoop);
                }

                FilledRegion filledRegion = FilledRegion.Create(RevitDoc.Instance.Doc, fillType.Id, activeView.Id, bounds);

                if (clipView != null)
                {
                    RevitDoc.Instance.Doc.Delete(clipView.Id);
                }

                tx.Commit();
            }
            

            return Result.Succeeded;
        }


        /// <summary>
        /// get all curves on geometry element(in revit, curves will be breaked when other elements cut in)
        /// </summary>
        /// <param name="geo"></param>
        /// <returns></returns>
        private List<Curve> GetGeoEdgeCurves(GeometryElement geo)
        {
            List<Curve> cs = new List<Curve>();
            if (geo == null) return cs;
            foreach(GeometryObject obj in geo)
            {
                Solid sol = obj as Solid;
                if(sol != null)
                {
                    EdgeArray edges = sol.Edges;

                    foreach(Edge edge in edges)
                    {
                        Curve c = edge.AsCurve();
                        cs.Add(c);
                    }
                }
                else
                {
                    GeometryInstance inst = obj as GeometryInstance;
                    if(inst != null)
                    {
                        cs.AddRange(GetGeoEdgeCurves(inst.GetInstanceGeometry()));
                    }
                    else
                    {
                        return cs;
                    }
                }
            }
            return cs;
        }

        

        void GetCurvesInPlane(List<Curve> curves,Plane plane,GeometryElement geo)
        {
            
            if (geo != null)
            {
                foreach (GeometryObject obj in geo)
                {
                    
                    Solid sol = obj as Solid;
                    
                    if (sol != null)
                    {
                        EdgeArray edges = sol.Edges;
                        foreach (Edge edge in edges)
                        {
                            Curve curve = edge.AsCurve();
                            Debug.Assert(curve is Line,"目前仅支持直线段填充");
                            
                            if (IsLineInPlane(curve as Line, plane))
                            {
                                curves.Add(curve);
                            }
                        }
                    }
                    else
                    {
                        GeometryInstance inst = obj as GeometryInstance;
                        if (null != inst)
                        {
                            GetCurvesInPlane(curves,plane, inst.GetInstanceGeometry());
                        }
                        else
                        {
                            Debug.Assert(false,
                              "unsupported geometry object "
                              + obj.GetType().Name);
                        }
                    }
                }
            }
        }

        

        /// <summary>
        /// Predicate returning true if the given line 
        /// lies in the given plane
        /// </summary>
        static bool IsLineInPlane(
          Line line,
          Plane plane)
        {
            XYZ p0 = line.GetEndPoint(0);
            XYZ p1 = line.GetEndPoint(1);
            UV uv0, uv1;
            double d0, d1;

            plane.Project(p0, out uv0, out d0);
            plane.Project(p1, out uv1, out d1);

            Debug.Assert(0 <= d0,
              "expected non-negative distance");
            Debug.Assert(0 <= d1,
              "expected non-negative distance");

            return (LineOnPlane_THRESHOLD > d0) && (LineOnPlane_THRESHOLD > d1);
        }

        public class SectionElementSelecion : ISelectionFilter
        {
            List<BuiltInCategory> categories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Roofs,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_Stairs,
                BuiltInCategory.OST_Floors,
            };
            public bool AllowElement(Element elem)
            {
                foreach(var ca in categories)
                {
                    if (elem.Category == null) return false;
                    if (elem.Category.Id.IntegerValue == (int)ca) return true;
                }
                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }


    }

    
}
