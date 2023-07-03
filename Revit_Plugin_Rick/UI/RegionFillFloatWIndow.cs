using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Revit_Plugin_Rick.UI
{
    /// <summary>
    /// Interaction logic for RegionFillFloatWIndow.xaml
    /// </summary>
    public partial class FilledRegionCreateFloatWIndow : Window
    {
        public FilledRegionCreateFloatWIndow()
        {
            InitializeComponent();
            new WindowInteropHelper(this).Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            Background = Brushes.Transparent;

            //make mouse event get through the window to other applications
            this.SourceInitialized += delegate
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            };
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private const int WS_EX_TRANSPARENT = 0x20;

        private const int GWL_EXSTYLE = -20;

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
    }
}
