using System;

namespace AnomalyDetectionWebService.Models.Types
{
    // { model_id: <int>, upload_time: <datetime>, status: “ready” | “pending” }
    // example for upload_time: 2021-04-22T19:15:32+02.00

    public class MODEL
    {
        // note that if model_id will be string then that is security issue,
        // since there are special char so it might affect file not only within Program.NormalModelsDBFolder,
        // like for example if someone request to delete "../../../../../../../../../../../../someImportantFileThatIsNotRelatedEvenToTheServerApp"
        public int model_id { get; set; }
        private DateTime time;
        public DateTime upload_time { get => time; set { time = value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond)); } }

        public string status { get; set; }

        public static readonly string Status_Pending = "pending";
        public static readonly string Status_Ready = "ready";
        public static readonly string Status_Corrupted = "corrupted";

        // this only return string that correspond to the model_id, the file may not exist
        public string FileName()
        {
            return Program.NormalModelsDBFolder + model_id + Program.NormalModelsFileExtension;
        }
    }
}
