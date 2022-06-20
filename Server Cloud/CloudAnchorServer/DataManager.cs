using extensionsClass;
using NetworkCommon;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Summary description for DataManager
/// Class for managing cloudanchor and corresponding Modeldata,... in memory for faster access.
/// </summary>
/// 


namespace DBCon
{

    class CheckRelease
    {
        bool reserved = false;
        DateTime timestamp = DateTime.Now;

        public CheckRelease()
        {
        }

        public void Set(bool status)
        {
            reserved = status;
            timestamp = DateTime.Now;
        }

        public bool Check()
        {
            TimeSpan interval = DateTime.Now - timestamp;

            if (interval.Seconds > 5)
                reserved = false;

            return reserved;
        }
    }

    public class DataManager
    {
        Dictionary<string, HashSet<string>> anchors = new Dictionary<string, HashSet<string>>();
        Dictionary<int,ModelData> models = new Dictionary<int, ModelData>();
        Dictionary<int, CheckRelease> modelsReserved = new Dictionary<int, CheckRelease>();

        private Object EditAnchorLock = new Object();
        private Object EditModelsLock = new Object();

        public DataManager()
        {
            DataBaseConnector dataBaseConnector = new DataBaseConnector(this);
            dataBaseConnector.ConnectToDB();

            anchors = dataBaseConnector.GetContextAnchors();

            ModelData[] models = dataBaseConnector.GetModels();

            if (models.Length > 0)
                InsertModels(models);
        }

        public Dictionary<string, HashSet<string>> GetContextAnchors()
        {
            return anchors;
        }

        public string[] GetContexts()
        {
            return anchors.Keys.ToArray();
        }

        public bool DeleteContext(string context)
        {
            bool result = false;
            if (anchors.ContainsKey(context))
            {
                List<string> anchorsList = new List<string>(GetAnchorsByContext(context));

                for (int i = models.Count - 1; i >= 0; i--)
                {
                    if (anchorsList.Contains(models[i].Id.anchorId))
                    {
                        models.Remove(models[i].Id.GetHashCode());
                    }
                }

                anchors.Remove(context);
                result = true;
            }

            return result;
        }

        public string[] GetAnchorsByContext(string context)
        {
            Console.WriteLine("Keys:");
            foreach (string key in anchors.Keys)
                Console.Write(" {0}",key);
            Console.Write("\n");

            if (anchors.ContainsKey(context))
                return anchors[context].ToArray();
            else
                return new string[0];
        }

        public bool InsertAnchor(string context, string id)
        {
            lock (EditAnchorLock)
            {
                if (!anchors.ContainsKey(context))
                    anchors.Add(context, new HashSet<string>());

                if (!anchors[context].Contains(id))
                {
                    anchors[context].Add(id);

                    return true;
                }
                return false;
            }
        }
        public bool DeleteAnchor(string context, string id)
        {
            lock (EditAnchorLock)
            {
                if (anchors[context].Contains(id))
                {
                    anchors[context].Remove(id);

                    ModelData[] models = GetModels(id, context);
                    DeleteModels(models);

                    return true;
                }
                return false;
            }
        }

        private bool SelectModel(ModelData model, string anchorId, string context, int task)
        {
            bool condition = false;
            switch (task)
            {
                case 0:
                    condition = model.Id.anchorId == anchorId;
                    break;
                case 1:
                    condition = model.Context == context;
                    break;
                case 2:
                    condition = model.Id.anchorId == anchorId && model.Context == context;
                    break;
                case 66:
                    condition = true;
                    break;
                default:
                    break;
            }
            return condition;
        }

        public ModelData[] GetModels(string anchorId, string context, int task = 0)
        {
            List<ModelData> selection = new List<ModelData>();

            foreach (ModelData model in models.Values)
            {
                if (SelectModel(model, anchorId, context, task))
                    selection.Add(model);
            }

            return selection.ToArray();
        }

        public bool[] InsertModels(ModelData[] newModels)
        {
            lock (EditModelsLock)
            {
                List<bool> res = new List<bool>();
                List<ModelData> pushModels = new List<ModelData>();

                foreach (ModelData model in newModels)
                {   // Insert
                    int id = model.Id.GetHashCode();
                    if (!models.ContainsKey(id))
                    {
                        models[id] = model;
                        modelsReserved.Add(id, new CheckRelease());
                        pushModels.Add(model);
                        res.Add(true);
                    }
                    else
                    {   // Update
                        //ModelData oldModel = models[id];

                        //if (!modelsReserved[id].Check() || oldModel.Update(model))
                        //{
                        //    res.Add(true);
                        //    modelsReserved[id].Set(true);
                        //    contexts.Add(model.Context);
                        //}
                        //else res.Add(false);
                        res.Add(false);
                    }
                }

                return res.ToArray();
            }
        }

        public bool[] UpdateModels(ModelData[] updateModels)
        {
            lock (EditModelsLock)
            {
                List<bool> res = new List<bool>();
                List<ModelData> pushModels = new List<ModelData>();

                foreach (ModelData model in updateModels)
                {
                    int id = model.Id.GetHashCode();
                    if (models.ContainsKey(id))
                    {   // Update
                        ModelData oldModel = models[id];

                        if (!modelsReserved[id].Check() && oldModel.Update(model))
                        {
                            pushModels.Add(model);
                            res.Add(true);
                            modelsReserved[id].Set(true);
                        }
                        else res.Add(false);
                    }
                    else
                    {   // Insert
                        //models[id] = model;
                        //modelsReserved.Add(id, new CheckRelease());
                        //res.Add(true);
                        //contexts.Add(model.Context);
                        res.Add(false);
                    }
                }

                return res.ToArray();
            }
        }

        public bool[] DeleteModels(ModelData[] delModels)
        {
            lock (EditModelsLock)
            {
                List<bool> res = new List<bool>();
                List<ModelData> pushModels = new List<ModelData>();

                foreach (ModelData model in delModels)
                {
                    int id = model.Id.GetHashCode();
                    if (models.ContainsKey(id))
                    {
                        models.Remove(id);
                        modelsReserved.Remove(id);
                        pushModels.Add(model);
                        res.Add(true);
                    }
                    else
                        res.Add(false);
                }

                return res.ToArray();
            }
        }

        public bool[] ReleaseModels(ModelData[] releaseModels)
        {
            lock (EditModelsLock)
            {
                List<bool> res = new List<bool>();

                foreach (ModelData model in releaseModels)
                {
                    int id = model.Id.GetHashCode();
                    if (models.ContainsKey(id))
                    {
                        modelsReserved[id].Set(false);
                        res.Add(true);
                    }
                    else
                        res.Add(false);
                }

                return res.ToArray();
            }
        }
    }
}

