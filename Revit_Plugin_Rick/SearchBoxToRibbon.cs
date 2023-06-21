using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

using Revit_Plugin_Rick.UI;

namespace Revit_Plugin_Rick
{
    class SearchBoxToRibbon : IExternalApplication
    {
        CommandFinder finder = CommandFinder.Instance;
        UIApplication uiapp;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;

            Autodesk.Revit.UI.RibbonPanel panel = CreateRibbonPanel(application, "BIM效率工具", "command");

            //search box
            TextBoxData textBoxData = new TextBoxData("SearchBoxText");
            var textBox_search = panel.AddItem(textBoxData) as TextBox;
            textBox_search.PromptText = "在此输入要搜索的命令";
            textBox_search.EnterPressed += new EventHandler<TextBoxEnterPressedEventArgs>(TextBox_search_EnterPressed);
            //button
            //PushButtonData searchButtonData = new PushButtonData("search", "search", Assembly.GetExecutingAssembly().Location, "Revit_Plugin_Rick.SearchBoxToRibbon");
            //PushButton searchButton = panel.AddItem(searchButtonData) as PushButton;
            //searchButton.LargeImage = new BitmapImage(new Uri(@"C:\Users\ricks\OneDrive\_EVENTS_\revit\Revit_Plugin_Rick\Revit_Plugin_Rick\PNG\search.png"));
            
            
            return Result.Succeeded;
        }

        void TextBox_search_EnterPressed(object sender,TextBoxEnterPressedEventArgs e)
        {
            var textBox = sender as TextBox;
            finder.Search_input = (string)textBox.Value;
            finder.Execute(uiapp);
            
        }

        private void ControlledApplication_ApplicationInitialized(object sender,ApplicationInitializedEventArgs e)
        {
            uiapp = new UIApplication(sender as Application);   
        }


        public RibbonPanel CreateRibbonPanel(UIControlledApplication a,string tabName,string panelName)
        {
            
            RibbonPanel rPanel = null;
            try
            {
                a.CreateRibbonTab(tabName);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            try
            {
                a.CreateRibbonPanel(tabName, panelName);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            List<RibbonPanel> panels = a.GetRibbonPanels(tabName);
            foreach(RibbonPanel p in panels.Where(p => p.Name == panelName))
            {
                rPanel = p;
            }

            return rPanel;
        }
    }
}
