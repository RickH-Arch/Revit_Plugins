using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Revit_Plugin_Rick.Utils
{
    public class MouseHook
    {
        private int hHook;
        public const int WH_MOUSE_LL = 14;
        
    }

    public class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        public class Point
        {
            public int x;
            public int y;
        }
        

    }
}
