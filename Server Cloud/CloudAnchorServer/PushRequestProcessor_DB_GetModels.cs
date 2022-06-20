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
    class PushRequestProcessor_DB_GetModels : PushRequestProcessor_DB
    {
        public PushRequestProcessor_DB_GetModels(DataManager dataManager) : base("getARModel", dataManager)
        {
        }

        public override void Notify(object o)
        {
            Console.WriteLine("Sending push updates...");

            for (int i = pushListener.Count - 1; i >= 0; i--)
            {
                try
                {
                    ModelResult result = (ModelResult)o;

                    GetARModelRequest request = (GetARModelRequest)pushListener[i].Item1;

                    if (request.AnchorId == result.Models[0].Id.anchorId)
                    {
                        request.SetResult(result.Models);

                        Response response = null;

                        if (result.Task == 3)
                            response = new DeleteResponse(request);
                        else
                            response = new Response(request);

                        Console.WriteLine("[{0}] Sending push update for GetARModelRequest to client No.{1}, #models {2}", DateTime.Now.ToString(), pushListener[i].Item2.GetClientNo(), result.Models.Length);

                        pushListener[i].Item2.SendResponse(response);
                    }
                }
                catch
                {
                    Console.WriteLine("[{0}] Client {1} disconnected", DateTime.Now.ToString(), pushListener[i].Item2.GetClientNo());
                    pushListener.Remove(pushListener[i]);
                }
            }

        }
    }
}

