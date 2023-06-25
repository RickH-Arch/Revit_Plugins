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
using System.Reflection;


namespace Revit_Plugin_Rick
{
    [Transaction(TransactionMode.Manual)]
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

        private CommandFrequencyRecorder recorder = CommandFrequencyRecorder.Instance;

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

        private UIApplication uiapp;
        private Document doc;
        private Application app;

        public Result Execute(UIApplication uiapp)
        {
            #region Get all commands
            //get all commands
            this.uiapp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
            app = uiapp.Application;
            
            

            if(cmdName.Count == 0)
            {
                //get revit command
                var names = Enum.GetNames(typeof(PostableCommand)).ToList();
                var values = new List<int>((IEnumerable<int>)Enum.GetValues(typeof(PostableCommand)));
                for (int i = 0; i < values.Count(); i++)
                {
                    try
                    {
                        var id = RevitCommandId.LookupPostableCommandId((PostableCommand)values[i]);
                        if (id != null && uiapp.CanPostCommand(id))
                        {
                            cmdId.Add(id);
                            cmdName.Add(names[i]);
                        }
                    }
                    catch { }
                }


                //get external command
                RibbonControl control = ComponentManager.Ribbon;
                foreach (var tab in control.Tabs)
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
                                    if(commandId != null)
                                    {
                                        if (!cmdId.Contains(commandId)&& uiapp.CanPostCommand(commandId))
                                        {
                                            bool hasSameId = false;
                                            foreach(var cmdid in cmdId)
                                            {
                                                if (cmdid.Id == commandId.Id)
                                                    hasSameId = true;
                                            }
                                            if (!hasSameId)
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
                }
               
                //seems no use
                /*IEnumerable<Assembly> loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach(Assembly assembly in loadedAssemblies)
                {
                    IEnumerable<Type> commandTypes = assembly.GetTypes().Where(type =>
            type.GetInterfaces().Contains(typeof(IExternalCommand)) &&
            type.GetCustomAttributes(typeof(TransactionAttribute), true).Length > 0);
                    foreach(Type commandType in commandTypes)
                    {
                        // 获取外部命令的名称
                        string commandName = GetCommandName(commandType);

                        // 获取外部命令的CommandId
                        string className = commandType.FullName;
                        RevitCommandId commandId = RevitCommandId.LookupCommandId(className);

                        
                    }
                }*/
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
            filtedCmdName = cmdName.FindAll(x => x.IndexOf(search_input,StringComparison.OrdinalIgnoreCase) >= 0);
            filtedCmdName.Sort(new StringSimilarityComparer(search_input));

            //get all cmdName contained in cmdFreqRecorder
            List<string> cmdNameCache = new List<string>();
            List<int> cmdFreqCache = new List<int>();
            for(int i = 0; i < filtedCmdName.Count; i++)
            {
                if (recorder.CmdFrequency.ContainsKey(filtedCmdName[i]))
                {
                    cmdNameCache.Add(filtedCmdName[i]);
                    cmdFreqCache.Add(recorder.CmdFrequency[filtedCmdName[i]]);
                    filtedCmdName.RemoveAt(i);
                    i--;
                }
            }
            var sortedList = cmdNameCache.OrderByDescending(str => cmdFreqCache[cmdNameCache.IndexOf(str)]).ToList();

            filtedCmdName.InsertRange(0, sortedList);


            if(BindingCmdName == null)
            {
                BindingCmdName = new ObservableCollection<string>(filtedCmdName);
            }
            else
            {
                BindingCmdName.Clear();
                foreach (var item in filtedCmdName)
                {
                    var id = cmdId[cmdName.IndexOf(item)];
                    BindingCmdName.Add(item);
                }
            }
        }

        public void PostCommandByName(string cmdN)
        {
            int ind = cmdName.IndexOf(cmdN);
            var cmdid = cmdId[ind];
            if (uiapp.CanPostCommand(cmdid))
            {
                uiapp.PostCommand(cmdid);
            }
            recorder.AddCommandFreq(cmdN);
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
