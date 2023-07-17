using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using AnomalyDetectionWebService.Models.Utils;
using System.Threading.Tasks;
using AnomalyDetectionWebService.Models.Types;
// Span should be list of 2 elements: [start_inclusive, end_exclusive]
using Span = System.Collections.Generic.List<long>;

namespace AnomalyDetectionWebService.Models
{
    // class to handle with list of normal models and detection of them
    //
    // safe-thread class
    // L_var should be access only within "lock (L_var){}" and for SHORT time lock!
    // for example L_NormalModels
    public class AnomalyDetectorsManager
    {
        // aviable model info, the total info like correlaion between features, is saved on system file in json files
        // there is no need to save this data in system file, becuse it's small (MODEL has 3 fields)
        // and safe thread writing/deleting with files / Database is more complicated
        private Dictionary<int, MODEL> L_NormalModels;

        // constructor to init L_NormalModels
        public AnomalyDetectorsManager(List<MODEL> initList)
        {
            L_NormalModels = new Dictionary<int, MODEL>();
            foreach (var model in initList)
                if (!L_NormalModels.ContainsKey(model.model_id))
                    L_NormalModels.Add(model.model_id,
                                          new MODEL() { model_id = model.model_id, 
                                                      status = model.status, upload_time = model.upload_time});
        }
        public bool IsReady(int modelId)
        {
            lock (L_NormalModels)
            {
                return L_NormalModels.ContainsKey(modelId) && L_NormalModels[modelId].status == MODEL.Status_Ready;
            }
        }
        public bool IsExist(int modelId)
        {
            lock (L_NormalModels)
            {
                return L_NormalModels.ContainsKey(modelId);
            }
        }
        public bool IsPending(int modelId)
        {
            lock (L_NormalModels)
            {
                return L_NormalModels.ContainsKey(modelId) && L_NormalModels[modelId].status == MODEL.Status_Pending;
            }
        }

        // try to learn new normal model and save it in correspond json file.
        // the learning is in new Task and the return MODEL status field is therfore "pending".
        // does afterFinishingLearning Action after the learning finished.
        public MODEL LearnAndAddNewModel(string detectoionType, Train_Data data, Action afterFinishingLearning) {
            MODEL model;
            // random new model_id
            Random rnd = new Random(DateTime.Now.Millisecond);
            int id = rnd.Next();
            // lock L_NormalModels in order to add the new mode_id
            lock (L_NormalModels)
            {
                // check it doesn't appear already, otherwise random new id.
                while (L_NormalModels.ContainsKey(id) || System.IO.File.Exists(new MODEL() { model_id = id}.FileName()))
                    id = rnd.Next();
                model = new MODEL() { model_id = id, status = MODEL.Status_Pending,
                                            upload_time = DateTime.Now };
                L_NormalModels.Add(id,model);
            }
            // start new task of learning correlative features
            Task.Run(() => {
                try
                {
                    // learn noraml mode [that might take while], only if id not deleted yet
                    var correlation = AnomalyDetection.GetNormal(data.train_data, detectoionType, ()=>this.IsExist(id));
                    if (correlation == null) throw new Exception();
                    // save it to json file
                    bool isSuccess = IO_Util.SaveNormalModel(model.FileName(), correlation, 
                                         new MODEL() {model_id = model.model_id, status = MODEL.Status_Ready, upload_time = model.upload_time });
                    
                    if (!isSuccess) throw new Exception();
                    lock (L_NormalModels)
                    {
                        if (L_NormalModels.ContainsKey(id)) {
                            L_NormalModels[id].status = MODEL.Status_Ready;
                        }
                        else
                        {
                            // probably we won't get here since correlaion wil be null if id was deleted from L_NormalModels,
                            // because we sent the lambda ()=>this.IsExist(id) to AnomalyDetection.GetNormal
                            try { System.IO.File.Delete(new MODEL() { model_id = id }.FileName()); }
                            catch { }
                        }

                    }
                } catch {
                    lock (L_NormalModels)
                    {
                        if (L_NormalModels.ContainsKey(id)) L_NormalModels[id].status = MODEL.Status_Corrupted;
                    }
                  }
                afterFinishingLearning();
                });
            // return MODEL even before the learning finished
            // { model_id = id, status = MODEL.Status_Pending, upload_time = DateTime.Now };
            return model;
         }

        // detect anomalies of given param data according to given model id.
        // this function is in one task, and might take time.
        // return null if failed.
        public ANOMALY Detect(int idModel, Predict_Data data)
        {
            string fileName = "";
            List<CorrelatedFeatures> correlation = null;
            lock(L_NormalModels)
            {
                // get file name (json file). within our server program deleting files,
                // is only via the beginning of Main or via AnomalyDetectorsManager_instance.Remove(), and only 1 instance exists,
                // but we lock L_NormalModels so it won't be removed meanwhile
                if (L_NormalModels.ContainsKey(idModel) && L_NormalModels[idModel].status == MODEL.Status_Ready)
                    fileName = L_NormalModels[idModel].FileName();
                if (String.IsNullOrWhiteSpace(fileName)) return null;
                // load normal model
                correlation = IO_Util.LoadNormalModel(fileName);
            }
            if (correlation == null) return null;
            try
            {
                // get detection (default detection method = "hybrid" , which it's enough if the learning was with hybrid\regression method)
                var detection = AnomalyDetection.GetDetection(data.predict_data, correlation);
                if (detection == null) return null;
                Dictionary<string, string> reason = AnomalyDetection.GetReportTypes(correlation, detection);
                Dictionary<String, List<Span>> spanDictionary = AnomalyDetection.ToSpanDictionary(detection);
                if (reason == null || spanDictionary == null) return null;
                return new ANOMALY() { anomalies = spanDictionary, reason = reason };
            }catch
            {
                return null;
            }

        }

        // get MODEL info from the dictionary L_NormalModels
        public MODEL getIdModel(int id)
        {
            lock (L_NormalModels)
            {
                if (L_NormalModels.ContainsKey(id))
                    return L_NormalModels[id];
                else
                    return null;
            }
        }
        // Remove MODEL info + correspond .json file
        public bool Remove(int id)
        {
            lock(L_NormalModels)
            {
                if (L_NormalModels.ContainsKey(id))
                {
                    L_NormalModels.Remove(id);
                    try { System.IO.File.Delete(new MODEL() { model_id = id }.FileName()); }
                    catch { }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        // get current models in the L_NoramlModels
        public List<MODEL> GetNormalModels()
        {
            lock (L_NormalModels)
            {
                var list = new List<MODEL>();
                foreach (var model in L_NormalModels.Values)
                        list.Add(model);
                return list;
            }
        }
    }
}