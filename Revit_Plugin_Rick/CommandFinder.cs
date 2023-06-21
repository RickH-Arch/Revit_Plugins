using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Windows;
using Revit_Plugin_Rick.UI;
using System.Collections.ObjectModel;

namespace Revit_Plugin_Rick
{
    public class CommandFinder : INotifyPropertyChanged,IExternalCommand
    {
        private static CommandFinder instance;

        public static CommandFinder Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new CommandFinder()
                    {
                        cmdName = new List<string>(),
                        cmdId = new List<RevitCommandId>()
                    };
                    
                }
                return instance;
            }

        }

        

        public event PropertyChangedEventHandler PropertyChanged;

        private string search_input;

        public string Search_input
        {
            get
            {
                return search_input;
            }
            set
            {
                search_input = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Search_input"));
                }
            }
        }

        private List<string> cmdName;
        private List<RevitCommandId> cmdId;
        private List<string> filtedCmdName;
        private ObservableCollection<string>bindingCmdName;

        public List<string> CmdName
        {
            get
            {
                return cmdName;
            }
        }

        public List<string> FiltedCmdName
        {
            get
            {
                return filtedCmdName;
            }
            set
            {
                filtedCmdName = value;
                //BindingCmdName = new ObservableCollection<string>(value);
            }
        }

        public ObservableCollection<string> BindingCmdName
        {
            get
            {
                return bindingCmdName;
            }
            set
            {
                bindingCmdName = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(BindingCmdName)));
                }
                
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }

        public Result Execute(UIApplication uiapp)
        {

            #region Get all commands
            //get all commands
            Document doc = uiapp.ActiveUIDocument.Document;
            Application app = uiapp.Application;
            RibbonControl control = ComponentManager.Ribbon;

            if(cmdName.Count == 0)
            {
                //get revit command
                cmdName.AddRange((IEnumerable<string>)Enum.GetNames(typeof(PostableCommand)));
                foreach(var e in Enum.GetValues(typeof(PostableCommand)))
                {
                    var id = RevitCommandId.LookupPostableCommandId((PostableCommand)e);
                    cmdId.Add(id);
                    
                }

                //get external command
                foreach(var tab in control.Tabs)
                {
                    foreach(var panel in tab.Panels)
                    {
                        RibbonItemEnumerator itor = panel.Source.GetItemEnumerator();
                        while (itor.MoveNext())
                        {
                            if(itor.Current is RibbonCommandItem cmd)
                            {
                                if(cmd.Name != null && !cmdName.Contains(cmd.Name))
                                {
                                    var commandId = RevitCommandId.LookupCommandId(cmd.Id);
                                    if (!cmdId.Contains(commandId))
                                    {

                                        cmdName.Add(cmd.Name);
                                        cmdId.Add(commandId);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion


            //refresh filtedCmdName
            RefreshFiltedCmdName();


            SearchCommandWindow window = new SearchCommandWindow();
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            window.ShowDialog();



            return Result.Succeeded;
        }

        public void RefreshFiltedCmdName()
        {
            FiltedCmdName = cmdName.FindAll(x => x.IndexOf(search_input,StringComparison.OrdinalIgnoreCase) >= 0);
            FiltedCmdName.Sort(new StringSimilarityComparer(search_input));
            if(BindingCmdName == null)
            {
                BindingCmdName = new ObservableCollection<string>(FiltedCmdName);
            }
            else
            {
                BindingCmdName.Clear();
                foreach (var item in FiltedCmdName)
                {
                    var id = cmdId[cmdName.IndexOf(item)];
                    BindingCmdName.Add(item);
                }
            }
        }

        class StringSimilarityComparer : IComparer<string>
        {

            public string search_input { get; set; }

            public StringSimilarityComparer(string search_input)
            {
                this.search_input = search_input;
            }

            public int Compare(string x, string y)
            {
                int similarityX = CalculateStringSimilarity(x, search_input);
                int similarityY = CalculateStringSimilarity(y, search_input);
                return similarityX.CompareTo(similarityY);
            }

            private int CalculateStringSimilarity(string str1,string str2)
            {
                int m = str1.Length;
                int n = str2.Length;

                // 创建一个二维数组用于存储子问题的解
                int[,] dp = new int[m + 1, n + 1];

                for (int i = 0; i <= m; i++)
                {
                    dp[i, 0] = i;
                }

                for (int j = 0; j <= n; j++)
                {
                    dp[0, j] = j;
                }

                // 动态规划求解
                for (int i = 1; i <= m; i++)
                {
                    for (int j = 1; j <= n; j++)
                    {
                        if (str1[i - 1] == str2[j - 1])
                        {
                            dp[i, j] = dp[i - 1, j - 1];
                        }
                        else
                        {
                            dp[i, j] = Math.Min(dp[i - 1, j], Math.Min(dp[i, j - 1], dp[i - 1, j - 1])) + 1;
                        }
                    }
                }
                return dp[m, n];
            }
        }

        


    }

    
}
