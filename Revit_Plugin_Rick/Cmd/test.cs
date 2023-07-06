/*using System;
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

namespace Revit_Plugin_Rick.Cmd
{
    class ScreenClientPointUtils
    {
        UIDocument _uiDocument = null;
        UIView _uiView = null;
        Transform _transfrom = Transform.Identity;
        public ScreenClientPointUtils(UIDocument uiDocument)
        {
            System.Diagnostics.Debug.Assert(uiDocument != null);

            _uiDocument = uiDocument;

            var activeView = uiDocument.Document.ActiveView;

            _uiView = uiDocument.GetOpenUIViews().First(o => o.ViewId == activeView.Id);

            _transfrom.Origin = activeView.Origin;
            _transfrom.BasisX = activeView.RightDirection;
            _transfrom.BasisY = activeView.UpDirection;
            _transfrom.BasisZ = activeView.ViewDirection;

            //if (activeView is ViewPlan)
            //    _uiView = uiDocument.GetOpenUIViews().First(o => o.ViewId == activeView.Id);
            //else
            //    throw new ArgumentException(activeView.GetType().FullName);
        }

        /// <summary>
        /// 屏幕坐标到revit平面坐标转换
        /// 
        /// 该接口没有考虑到屏幕DPI，就是说在分辨率不在100%时，可能有问题
        /// 
        /// 参考 ViewPlan2Screen接口
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public XYZ Screen2ViewPlan(System.Drawing.Point screenPoint)
        {
            //屏幕坐标
            var rect = _uiView.GetWindowRectangle();
            //屏幕比例
            double sWidth = rect.Right - rect.Left;
            double sHeight = rect.Bottom - rect.Top;

            double widthScale = (screenPoint.X - rect.Left) / sWidth;
            double heightScale = (rect.Bottom - screenPoint.Y) / sHeight;

            var corners = _uiView.GetZoomCorners();
            XYZ wLeftBottom = corners[0];
            XYZ wRightTop = corners[1];

            double wWidth = wRightTop.X - wLeftBottom.X;
            double wHeight = wRightTop.Y - wLeftBottom.Y;

            double widthDis = wWidth * widthScale;
            double heightDis = wHeight * heightScale;

            return new XYZ(wLeftBottom.X + widthDis, wLeftBottom.Y + heightDis, 0);
        }
        /// <summary>
        /// revit长度转屏幕像素长度
        /// </summary>
        /// <param name="length">单位: 英尺</param>
        /// <returns></returns>
        public double ViewPlanLength2ScreenPixel(double length)
        {
            XYZ wLeftBottom, wRightTop;
            GetWorkspaceRect(out wLeftBottom, out wRightTop);

            var bottomLeft = ViewPlan2Screen(wLeftBottom);
            var topRight = ViewPlan2Screen(wRightTop);

            return length * (topRight.X - bottomLeft.X) / (wRightTop.X - wLeftBottom.X);
        }

        /// <summary>
        /// revit平面坐标到屏幕坐标转化
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public System.Drawing.Point ViewPlan2Screen(XYZ point)
        {
            var corners = _uiView.GetZoomCorners();

            XYZ wLeftBottom = _transfrom.Inverse.OfPoint(corners[0]);
            XYZ wRightTop = _transfrom.Inverse.OfPoint(corners[1]);
            var tmpPt = _transfrom.Inverse.OfPoint(point);

            double wWidth = wRightTop.X - wLeftBottom.X;
            double wHeight = wRightTop.Y - wLeftBottom.Y;

            double widthScale = (tmpPt.X - wLeftBottom.X) / wWidth;
            double heightScale = (tmpPt.Y - wLeftBottom.Y) / wHeight;

            //屏幕坐标
            var rect = _uiView.GetWindowRectangle();
            //屏幕比例
            double sWidht = rect.Right - rect.Left;
            double sHeight = rect.Bottom - rect.Top;

            double widthDis = sWidht * widthScale;
            double heightDis = sHeight * heightScale;

            return new System.Drawing.Point(
                (int)((rect.Left + widthDis) * 1 / DPICache.Instance.ScreenRatio),
                (int)((rect.Bottom - heightDis) * 1 / DPICache.Instance.ScreenRatio));
        }
        /// <summary>
        /// 获取revit工作区域的屏幕坐标
        /// </summary>
        /// <param name="bottomLeft"></param>
        /// <param name="topRight"></param>
        public void GetWorkspaceRect(
            out System.Drawing.Point bottomLeft,
            out System.Drawing.Point topRight)
        {
            XYZ wLeftBottom, wRightTop;
            GetWorkspaceRect(out wLeftBottom, out wRightTop);

            bottomLeft = ViewPlan2Screen(wLeftBottom);
            topRight = ViewPlan2Screen(wRightTop);
        }
        void GetWorkspaceRect(out XYZ bottomLeft, out XYZ topRight)
        {
            var corners = _uiView.GetZoomCorners();

            bottomLeft = corners[0];
            topRight = corners[1];
        }
        *//*class DPICache
        {
            DPICache()
            {
                Win32Api.SetProcessDPIAware();

                IntPtr screenDC = Win32Api.GetDC(IntPtr.Zero);
                int dpi_x = Win32Api.GetDeviceCaps(screenDC, *//*DeviceCap.*//*LOGPIXELSX);
                int dpi_y = Win32Api.GetDeviceCaps(screenDC, *//*DeviceCap.*//*LOGPIXELSY);
                //_scaleUI.X = dpi_x / 96.0;
                //_scaleUI.Y = dpi_y / 96.0;
                Win32Api.ReleaseDC(IntPtr.Zero, screenDC);
                System.Diagnostics.Debug.Assert(dpi_x == dpi_y);
                if (dpi_x != dpi_y)
                {
                    Framework.Logger.Instance.Error(
                        string.Format("获取屏幕DPI失败, dpi_x: {0} dpi_x: {1} ", dpi_x, dpi_y));
                }

                ScreenRatio = dpi_x / 96.0;
            }
            public static readonly DPICache Instance = new DPICache();
            public double ScreenRatio { get; private set; }

            const int LOGPIXELSX = 88;
            const int LOGPIXELSY = 90;
        }*//*
    }
}
*/