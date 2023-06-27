using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Revit_Plugin_Rick
{
    class CommandFrequencyRecorder
    {
        private static CommandFrequencyRecorder instance;
        public static CommandFrequencyRecorder Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new CommandFrequencyRecorder();
                }
                return instance;
            }
        }

        private string jsonPath = @"cmdFrequency.json";

        private Dictionary<string, int> cmdFrequency;
        public Dictionary<string,int> CmdFrequency
        {
            get
            {
                return cmdFrequency;
            }
        }

        public CommandFrequencyRecorder()
        {
            string json = null;
            try
            { 
                json = File.ReadAllText(jsonPath);
            }
            catch { }
            
            if (json == null)
            {
                cmdFrequency = new Dictionary<string, int>();

            }
            else
            {
                cmdFrequency = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
            }
        }

        public void AddCommandFreq(string command)
        {
            if (cmdFrequency.ContainsKey(command))
            {
                cmdFrequency[command] += 1;
            }
            else
            {
                cmdFrequency.Add(command, 1);
            }
            WriteJson(jsonPath);
        }

        private void WriteJson(string path)
        {
            string json = JsonConvert.SerializeObject(cmdFrequency, Formatting.Indented);
            File.WriteAllText(path,json);
        }

            

    }
}
