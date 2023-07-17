using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnomalyDetectionWebService.Models.Types;


namespace AnomalyDetectionWebService.Models.Utils
{
    public class ExtendedModelInfo
    {
        public MODEL info { get; set; }
        public List<CorrelatedFeatures> normal_model { get; set; }
    }
    public class IO_Util
    {
        // save normal model (list of correlated features) and their descrition/info to info file. return false if error occurs.
        public static bool SaveNormalModel(string outputName, List<CorrelatedFeatures> normal_model, MODEL description)
        {
            ExtendedModelInfo info = new ExtendedModelInfo() { info = description, normal_model = normal_model };
            try
            {
                string jsonString = JsonSerializer.Serialize(info);
                File.WriteAllText(outputName, jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
        // return normal model (list of correlated features) from info file. return null if error occurs.
        public static List<CorrelatedFeatures> LoadNormalModel(string sourceFile)
        {
            return RestoreExtendedModelInfo(sourceFile)?.normal_model;
        }
        // return normal model (list of correlated features) and their descrition/info from info file. return null if error occurs.
        public static ExtendedModelInfo RestoreExtendedModelInfo(string sourceFile)
        {
            try
            {
                string jsonString = File.ReadAllText(sourceFile);
                return JsonSerializer.Deserialize<ExtendedModelInfo>(jsonString);
            }
            catch
            {
                return null;
            }
        }
    }
}
