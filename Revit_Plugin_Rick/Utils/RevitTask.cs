using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit_Plugin_Rick.Utils
{
    internal class RevitTask
    {
        private ExternalEventHandler Handler { get; set; }

        private ExternalEvent ExternalEvent { get; set; }

        public RevitTask()
        {
            Handler = new ExternalEventHandler();
            ExternalEvent = ExternalEvent.Create(Handler);
        }

        public void Run(Action<UIApplication> action)
        {
            Handler.Action = action;
            ExternalEvent.Raise();
        }

        internal class ExternalEventHandler : IExternalEventHandler
        {
            public Action<UIApplication> Action { get; set; }
            public void Execute(UIApplication app)
            {
                try
                {
                    Action(app);
                }
                catch(Exception ex)
                {
                    MessageBox.Show(GetName(), ex.Message);
                }
            }

            public string GetName()
            {
                return "Revit Task";
            }
        }
    }
}
