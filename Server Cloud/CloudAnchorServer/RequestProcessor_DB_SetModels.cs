using System;
using System.Collections.Generic;
using System.Linq;
using NetworkCommon;
using DBCon;

namespace CloudAnchorServer
{
    public class ModelResult : Object
    {
        internal int Task { get; private set; }
        internal ModelData[] Models { get; private set; }

        public ModelResult(int task, ModelData[] models) : base()
        {
            this.Task = task;
            this.Models = models;
        }
    }

    class RequestProcessor_DB_SetModels : RequestProcessor_DB
    {
        public RequestProcessor_DB_SetModels(DataManager dataManager, PushRequestProcessor pushRequestProcessor) : base("setARModel", dataManager, pushRequestProcessor) { }
        protected override void SendResponse(DataRequest request, HandleClient client)
        {
            Console.WriteLine("[{0}] Received SetARModelRequest from client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            bool[] result = null;

            SetARModelRequest r = (SetARModelRequest)request;

            ModelData[] models = r.Models;

            Console.WriteLine("Save -> models");

            switch (r.Task)
            {
                case 0:
                    result = dataManager.InsertModels(models);
                    Console.WriteLine("Insert -> {0} of {1} models.", result.Count(c => c), models.Length);
                    break;
                case 1:
                    result = dataManager.UpdateModels(models);
                    Console.WriteLine("Update -> {0} of {1} models.", result.Count(c => c), models.Length);
                    break;
                case 2:
                    result = dataManager.ReleaseModels(models);
                    Console.WriteLine("Release -> {0} of {1} models.", result.Count(c => c), models.Length);
                    break;
                case 3:
                    result = dataManager.DeleteModels(models);
                    Console.WriteLine("Delete -> {0} of {1} models.", result.Count(c => c), models.Length);
                    break;
                default:
                    Console.WriteLine($"Unable to process task {r.Task} for Request setAnchor.");
                    break;
            }

            Console.WriteLine("[{0}] Sending response for SetARModelRequest to client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            r.SetResult(result);
            client.SendResponse(new Response(r));


            if (result.Where(c => c).Count() > 0)
            {
                List<ModelData> list = new List<ModelData>();

                for (int i = 0; i < result.Length; i++)
                    if (result[i])
                        list.Add(models[i]);

                this.PushRequestProcessor.Notify(new ModelResult(r.Task, list.ToArray()));
            }
        }

        protected override bool IsHandleRequest(object r) { return r is SetARModelRequest; }
    }
}

