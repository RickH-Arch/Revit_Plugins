/*using System;
using System.Text;
using System.Linq;

using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

namespace Revit_Plugin_Rick.Cmd
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    public class UpdateBrickDims : IExternalApplication
    {
        /// <summary>
        /// On shutdown, unregister the updater
        /// </summary>
        /// 

        public Result OnShutdown(UIControlledApplication a)
        {
            BrickDimUpdater updater = new BrickDimUpdater(a.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }

        /// <summary>
        /// On start up, add UI buttons, register 
        /// the updater and add triggers
        /// </summary>
        public Result OnStartup(UIControlledApplication a)
        {
            // Add the UI buttons on start up
            AddButtons(a);

            BrickDimUpdater updater = new BrickDimUpdater(a.ActiveAddInId);

            // Register the updater in the singleton 
            // UpdateRegistry class
            UpdaterRegistry.RegisterUpdater(updater);


            // Set the filter
            //ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            //ElementOwnerViewFilter filterView = new ElementOwnerViewFilter(doc.ActiveView.Id);
            //ElementCategoryFilter filter = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
            //LogicalAndFilter filter = new LogicalAndFilter(filterView, filterClass);
            // Add trigger 
            //UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), filter, Element.GetChangeTypeAny());
            if (!UpdaterRegistry.IsUpdaterRegistered(updater.GetUpdaterId()))
            {
                UpdaterRegistry.RegisterUpdater(updater, true);
                ElementCategoryFilter f = new ElementCategoryFilter(BuiltInCategory.OST_Dimensions);
                UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), f, Element.GetChangeTypeAny());
            }
            return Result.Succeeded;
        }

        /// <summary>
        /// Add UI buttons
        /// </summary>
        public void AddButtons(UIControlledApplication a)
        {
            // create a ribbon panel on the Analyze tab
            RibbonPanel panel = a.CreateRibbonPanel(
              Tab.AddIns, "RST Labs");

            AddDmuCommandButtons(panel);
        }


        /// <summary>
        /// Control buttons for the Dynamic Model Update 
        /// </summary>
        public void AddDmuCommandButtons(RibbonPanel panel)
        {
            string path = GetType().Assembly.Location;

            string sDirectory = System.IO.Path.GetDirectoryName(path);

            // create toggle buttons for radio button group 

            ToggleButtonData toggleButtonData3
              = new ToggleButtonData(
                "RSTLabsDMUOff", "Align Off", path,
                "BrickDims.UIDynamicModelUpdateOff");


            ToggleButtonData toggleButtonData4
              = new ToggleButtonData(
                "RSTLabsDMUOn", "Align On", path,
                "BrickDims.UIDynamicModelUpdateOn");

            // make dyn update on/off radio button group 

            RadioButtonGroupData radioBtnGroupData2 =
              new RadioButtonGroupData("RebarAlign");

            RadioButtonGroup radioBtnGroup2
              = panel.AddItem(radioBtnGroupData2)
                as RadioButtonGroup;

            radioBtnGroup2.AddItem(toggleButtonData3);
            radioBtnGroup2.AddItem(toggleButtonData4);
        }
    }

    public class BrickDimUpdater : IUpdater
    {
        public static bool m_updateActive = false;
        AddInId addinID = null;
        UpdaterId updaterID = null;

        public BrickDimUpdater(AddInId id)
        {
            addinID = id;
            // UpdaterId that is used to register and 
            // unregister updaters and triggers
            updaterID = new UpdaterId(addinID, new Guid(
              "63CDBB88-5CC4-4ac3-AD24-52DD435AAB25"));
            Console.WriteLine("Standard Numeric Format Specifiers3");
        }

        public int caseswitch = 1;

        /// <summary>
        /// Colour Code Dimensions
        /// </summary>
        public void Execute(UpdaterData data)
        {
            TaskDialog.Show("case 2", "Check");
            if (m_updateActive == false) { return; }

            // Get access to document object
            Document doc = data.GetDocument();
            UIDocument uidoc = new UIDocument(doc);

            //using (Transaction t = new Transaction(doc, "Update Dim"))
            //{
            //   t.Start();
            try
            {
                TaskDialog.Show("case 2", "Check");
                switch (caseswitch)
                {
                    case 1:
                        // Loop through all the modified elements
                        ICollection<ElementId> modifiedCollection = data.GetModifiedElementIds();
                        foreach (ElementId elemId in modifiedCollection)
                        {
                            Dimension dim = doc.GetElement(elemId) as Dimension;
                            //DimClrChecker.DimClrChngSuffix(dim, uidoc);
                        }
                        caseswitch = 2;
                        break;

                    case 2:
                        TaskDialog.Show("case 2", "SampleUpdater");

                        caseswitch = 1;
                        break;
                }

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Exception", ex.Message);
            }

            //t.Commit();
            //}
        }
        /// <summary>
        /// Set the priority
        /// </summary>
        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        /// <summary>
        /// Return the updater Id
        /// </summary>
        public UpdaterId GetUpdaterId()
        {
            return updaterID;
        }

        /// <summary>
        /// Return the auxiliary string
        /// </summary>
        public string GetAdditionalInformation()
        {
            return "Automatically update dimension overrides";
        }

        /// <summary>
        /// Return the updater name
        /// </summary>
        public string GetUpdaterName()
        {
            return "Dimension Override Updater";
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class UIDynamicModelUpdateOff : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BrickDimUpdater.m_updateActive = false;
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.ReadOnly)]
    [Regeneration(RegenerationOption.Manual)]
    public class UIDynamicModelUpdateOn : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BrickDimUpdater.m_updateActive = true;
            return Result.Succeeded;
        }
    }
}
*/