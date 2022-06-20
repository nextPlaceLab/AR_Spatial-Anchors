using System;
using System.Collections.Generic;
using DBCon;
using NetworkCommon;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 

namespace CloudAnchorServer
{
    class RequestProcessor_DB_GetModels : RequestProcessor_DB
    {
        public RequestProcessor_DB_GetModels(DataManager dataManager, PushRequestProcessor pushRequestProcessor) : base("getARModel", dataManager, pushRequestProcessor)
        {
        }

        public override void OnRequest(DataRequest request, HandleClient client)
        {
            if (IsHandleRequest(request))
            {
                Console.WriteLine("[{0}] Received GetARModelRequest from client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

                GetARModelRequest r = (GetARModelRequest)request;

                ModelData[] models = this.dataManager.GetModels(r.AnchorId, r.ModelContext, r.Task);
                r.SetResult(models);

                if (r.IsUpdateListener)
                {
                    Console.WriteLine("Added client No.{0} to getARModel-PushListener", client.GetClientNo());
                    this.PushRequestProcessor.OnRequest(r, client);
                }

                Console.WriteLine("[{0}] Sending response for GetARModelRequest to client No.{1}, #models {2}", DateTime.Now.ToString(), client.GetClientNo(), models.Length);

                Response response = new Response(r);
                client.SendResponse(response);
            }
        }
        protected override bool IsHandleRequest(object r) { return r is GetARModelRequest; }
    }
}

