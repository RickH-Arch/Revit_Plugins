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

using Revit_Plugin_Rick.Extension;
using Revit_Plugin_Rick.Utils;
using Revit_Plugin_Rick.Utils.CurveUtils;
using Revit_Plugin_Rick.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Revit_Plugin_Rick.UI
{
    /// <summary>
    /// Interaction logic for RegionFillWindow.xaml
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public partial class FilledRegionCreateWindow : Window
    {

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        //第一个值为虚拟键值，第二个参数为扫描不设置为0，第三个数为按键状态选项 keydown为0，如果为keyup 则设置成2，KEYEVENT_KEYUP,第四个参数一般为0
        internal static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public delegate void UnRegister();
        public UnRegister unRegister;

        public FilledRegionCreateWindow(UnRegister unrigister)
        {
            InitializeComponent();
            unRegister = unrigister;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //unRegister.Invoke();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            unRegister.Invoke();
            SendESCToRevit();
        }

        public static void SendESCToRevit()
        {
            IntPtr Revit = Autodesk.Windows.ComponentManager.ApplicationWindow;
            SetForegroundWindow(Revit);
            keybd_event(0x1B, 0, 0, 0);
            keybd_event(0x1B, 0, 2, 0);
            keybd_event(0x1B, 0, 0, 0);
            keybd_event(0x1B, 0, 2, 0);
        }
    }
}
