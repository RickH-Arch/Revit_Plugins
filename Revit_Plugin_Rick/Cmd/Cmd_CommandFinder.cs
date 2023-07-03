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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Revit_Plugin_Rick
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CommandFinder : INotifyPropertyChanged,IExternalCommand
    {
        private static Cmd_CommandFinder instance;

        public static Cmd_CommandFinder Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new Cmd_CommandFinder()
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
        private ObservableCollection<RevitCommandInfoWrap> cmdInfoWrap;

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

        public ObservableCollection<RevitCommandInfoWrap> CmdInfoWrap
        {
            get
            {
                return cmdInfoWrap;
            }
            set
            {
                cmdInfoWrap = value;
                if(this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CmdInfoWrap)));
                }
            }
        }

        List<ImageSource> image_sources = new List<ImageSource>();
        List<string> image_names = new List<string>();
        List<RevitCommandId> image_commandId = new List<RevitCommandId>();


        //====================================================================================================//

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Execute(commandData.Application);
        }

        private UIApplication uiapp;
        private Document doc;
        private Application app;

        public Result Execute(UIApplication uiapp)
        {
            
            
            this.uiapp = uiapp;
            doc = uiapp.ActiveUIDocument.Document;
            app = uiapp.Application;

            #region Get all commands
            //get all commands
            if (cmdName.Count == 0)
            {
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
                            if(itor.Current.GetType() == typeof(RibbonCheckBox))
                            {
                                continue;
                            }
                            if (itor.Current.GetType() == typeof(RibbonSplitButton))
                            {
                                var splitButton = itor.Current as RibbonSplitButton;
                                var buttons = splitButton.Items;
                                foreach(var button in buttons)
                                {
                                    var b= button as RibbonCommandItem;
                                    if(b != null)
                                    {
                                        CollectIconInfo(b);
                                        AddCmdNameAndId(b);
                                    }
                                }
                            }
                            else
                            {
                                var cmd = itor.Current as RibbonCommandItem;
                                if(cmd!= null)
                                {
                                    CollectIconInfo(cmd);
                                    AddCmdNameAndId(cmd);
                                }
                            }
                        }
                    }
                }

                #region test function
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

                /*#region get command icon and id
                foreach(var tab in control.Tabs)
                {
                    foreach(var p in tab.Panels)
                    {
                        RibbonItemEnumerator itor = p.Source.GetItemEnumerator();
                        while (itor.MoveNext())
                        {
                            if (itor.Current is RibbonCommandItem cmd)
                            {
                                bitmaps.Add(ImageSourceToBitmap(cmd.LargeImage));
                                bitmap_names.Add(cmd.Name);
                                bitmap_commandId.Add(RevitCommandId.LookupCommandId(cmd.Id));
                            }
                        }
                    }
                }

                #endregion*/
                #endregion
            }

            #endregion



            //refresh filtedCmdName
            RefreshFiltedCmdName();


            SearchCommandWindow window = new SearchCommandWindow();
            window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            window.ShowDialog();



            return Result.Succeeded;
        }

        private void AddCmdNameAndId(RibbonCommandItem cmd)
        {
            string button_name = cmd.AutomationName;
            if (button_name.Contains("\r"))
            {
                button_name = button_name.Replace("\r", "");
            }
            if (button_name.Contains("\n"))
            {
                button_name = button_name.Replace("\n", "");
            }
            if (button_name != null && button_name != "" && !cmdName.Contains(button_name))
            {
                if(cmd.Id == "")
                {
                    return;
                }
                var commandId = RevitCommandId.LookupCommandId(cmd.Id);
                if (commandId != null && uiapp.CanPostCommand(commandId))
                {
                    
                    int idx = -1;
                    for(int i = 0; i < cmdId.Count; i++)
                    {
                        if(cmdId[i].Id == commandId.Id)
                        {
                            idx = i;
                        }
                    }

                    if(idx != -1)
                    {
                        cmdName[idx] = button_name;
                    }
                    else
                    {
                        cmdName.Add(button_name);
                        cmdId.Add(commandId);
                    }
                    
                }

            }
        }

        /// <summary>
        /// collect all button icons in ribbon, only run once
        /// </summary>
        /// <param name="cmd"></param>
        private void CollectIconInfo(RibbonCommandItem cmd)
        {
            if(cmd.Id == "")
            {
                return;
            }
            var commandId = RevitCommandId.LookupCommandId(cmd.Id);
            if(commandId != null)
            {
                image_sources.Add(cmd.LargeImage);
                image_names.Add(cmd.AutomationName);
                image_commandId.Add(commandId);
            }
        }


        /// <summary>
        /// 刷新过滤出的结果
        /// </summary>
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

            if(CmdInfoWrap == null)
            {
                CmdInfoWrap = new ObservableCollection<RevitCommandInfoWrap>();
            }
            CmdInfoWrap.Clear();
            foreach(var name in filtedCmdName)
            {
                var id = cmdId[cmdName.IndexOf(name)];
                CmdInfoWrap.Add(new RevitCommandInfoWrap(id));
            }
        }

        /// <summary>
        /// 根据名称触发命令
        /// </summary>
        /// <param name="cmdN"></param>
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


        /// <summary>
        /// 打包Revit Command信息
        /// </summary>
        public class RevitCommandInfoWrap
        {
            public string Name{get;set ;}
            public RevitCommandId CommandId { get; set; }
            public ImageSource ImageSource { get; set; }

            public RevitCommandInfoWrap(RevitCommandId commandId)
            {
                int ind = Cmd_CommandFinder.Instance.cmdId.IndexOf(commandId);
                if (ind != -1)
                {
                    this.Name = Cmd_CommandFinder.Instance.cmdName[ind];
                    this.CommandId = commandId;
                    int ndx = -1;
                    for(int i = 0; i < Cmd_CommandFinder.Instance.image_commandId.Count; i++)
                    {
                        if(Cmd_CommandFinder.Instance.image_commandId[i].Id == commandId.Id)
                        {
                            ndx = i;
                            break;
                        }
                    }
                    if(ndx != -1)
                    {
                        this.ImageSource = Cmd_CommandFinder.Instance.image_sources[ndx];
                    }
                    else
                    {
                        this.ImageSource = new BitmapImage(new Uri(Path.GetDirectoryName(typeof(Cmd_CommandFinder).Assembly.Location)+"\\Resources\\Bitmaps\\empty.bmp"));
                    }
                }
            }
        }


    }

    
}
