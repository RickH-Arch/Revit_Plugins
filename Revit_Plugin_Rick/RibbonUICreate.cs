﻿using System;
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
    class RibbonUICreate : IExternalApplication
    {
        Cmd_CommandFinder finder = Cmd_CommandFinder.Instance;
        UIApplication uiapp;

        public static TextBox textBox_search;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;
            
            Autodesk.Revit.UI.RibbonPanel panel = CreateRibbonPanel(application, "BIM效率工具", "search");

            //--- command search ---
            //search box
            TextBoxData textBoxData = new TextBoxData("SearchBoxText");
            textBox_search = panel.AddItem(textBoxData) as TextBox;
            textBox_search.Width = 200;
            textBox_search.PromptText = "在此输入要搜索的命令";
            textBox_search.EnterPressed += new EventHandler<TextBoxEnterPressedEventArgs>(TextBox_search_EnterPressed);
            //button
            //PushButtonData searchButtonData = new PushButtonData("search", "search", Assembly.GetExecutingAssembly().Location, "Revit_Plugin_Rick.SearchBoxToRibbon");
            //PushButton searchButton = panel.AddItem(searchButtonData) as PushButton;
            //searchButton.LargeImage = new BitmapImage(new Uri(@"C:\Users\ricks\OneDrive\_EVENTS_\revit\Revit_Plugin_Rick\Revit_Plugin_Rick\PNG\search.png"));


            //--- region fill create---
            panel = CreateRibbonPanel(application, "BIM效率工具", "fill");
            //button
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string regionFillClass = "Revit_Plugin_Rick.Cmd_FilledRegionGreate";

            PushButtonData btnd_regionFill = new PushButtonData("创建填充", "创建填充", assemblyPath, regionFillClass);
            PushButton btn_regionFill = panel.AddItem(btnd_regionFill) as PushButton;
            btn_regionFill.LargeImage = new BitmapImage(new Uri(@"C:\Users\ricks\OneDrive\_EVENTS_\revit\Revit_Plugin_Rick\Revit_Plugin_Rick\Resources\PNG\区域.png"));

            //--- region fill from element
            string assemblyPath2 = Assembly.GetExecutingAssembly().Location;
            string regionFillClass2 = "Revit_Plugin_Rick.Cmd_FilledRegionFromElement";
            PushButtonData btnd_regionFillFromElem = new PushButtonData("元素填充", "元素填充", assemblyPath2, regionFillClass2);
            PushButton btn_regionFillFromElem = panel.AddItem(btnd_regionFillFromElem) as PushButton;


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
