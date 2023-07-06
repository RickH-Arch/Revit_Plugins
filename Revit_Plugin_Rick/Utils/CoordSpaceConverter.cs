using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.Utils;
using Revit_Plugin_Rick.Utils.CurveUtils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;

namespace Revit_Plugin_Rick.Utils
{
    class CoordSpaceConverter
    {
        private Transform model2Screen_M;
        public Transform Model2Screen_M
        {
            get
            {
                return model2Screen_M;
            }
        }

        private Transform screen2Model_M;
        public Transform Screen2Model_M
        {
            get
            {
                return screen2Model_M;
            }
        }

        public CoordSpaceConverter(UIDocument uiDoc)
        {
            model2Screen_M = Transform.Identity;
            screen2Model_M = Transform.Identity;

            if (uiDoc == null) return;
            View activeView = uiDoc.ActiveView;
            if (activeView == null) return;
            UIView uiView = uiDoc.GetOpenUIViews().FirstOrDefault(p => p.ViewId.IntegerValue == activeView.Id.IntegerValue);
            if (uiView == null) return;
            Rectangle windowRec = uiView.GetWindowRectangle();
            IList<XYZ> zoomCorners = uiView.GetZoomCorners();
            if (zoomCorners.Count < 2) return;
            double cornerDis = zoomCorners[0].DistanceTo(zoomCorners[1]);
            if (cornerDis < uiDoc.Application.Application.ShortCurveTolerance)return;
            double dScale = Hypot(windowRec.Right - windowRec.Left, windowRec.Bottom - windowRec.Top) / cornerDis;
            Transform matCrop = activeView.CropBox.Transform;

            //model2Screen_M.BasisX = new XYZ(matCrop.BasisX.X, -matCrop.BasisY.X, 0);
            //model2Screen_M.BasisY = new XYZ(matCrop.BasisX.Y, -matCrop.BasisY.Y, 0);
            //model2Screen_M.BasisZ = new XYZ(matCrop.BasisX.Z, -matCrop.BasisY.Z, 1);
            //model2Screen_M.Origin = new XYZ(-zoomCorners[0].DotProduct(matCrop.BasisX), zoomCorners[1].DotProduct(matCrop.BasisY), 0);
            //model2Screen_M.ScaleBasisAndOrigin(dScale);
            //model2Screen_M.Origin += new XYZ(windowRec.Left, windowRec.Top, 0);

            screen2Model_M.Origin = activeView.Origin;
            screen2Model_M.BasisX = activeView.RightDirection;
            screen2Model_M.BasisY = activeView.UpDirection;
            screen2Model_M.BasisZ = activeView.ViewDirection;

            model2Screen_M = screen2Model_M.Inverse;
        }

        public XYZ Model2Screen(XYZ point)
        {
            if (model2Screen_M == null) return null;
            XYZ screenPoint = model2Screen_M.OfPoint(point);
            //screenPoint = new XYZ(screenPoint.X, screenPoint.Y, 0);
            return screenPoint;
        }

        public XYZ Screen2Model(XYZ point)
        {
            if (screen2Model_M == null) return null;
            return screen2Model_M.OfPoint(point);
        }

        private double Hypot(params double[] arD)
        {
            double dSum = 0;
            foreach (double dT in arD)
            {
                dSum += Math.Pow(dT, 2);
            }
            return Math.Sqrt(dSum);
        }
    }
}
