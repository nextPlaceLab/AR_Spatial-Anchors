using System;
using DBCon;
using NetworkCommon;

/// <summary>
/// Summary description for PushRequestProcessor_DB_GetAnchors
/// </summary>
/// 
namespace CloudAnchorServer
{
    class PushRequestProcessor_DB_GetAnchors : PushRequestProcessor_DB
    {
        public PushRequestProcessor_DB_GetAnchors(DataManager dataManager) : base("getAnchor", dataManager)
        {
        }

        public override void Notify(object o)
        {
            Console.WriteLine("Sending push updates...");

            for (int i = pushListener.Count - 1; i >= 0; i--)
            {
                try
                {
                    AnchorResult result = (AnchorResult)o;

                    GetAnchorRequest request = (GetAnchorRequest)pushListener[i].Item1;

                    if (request.AnchorContext == result.Context)
                    {
                        request.SetResult(new string[] { result.AnchorId });

                        Response response = null;

                        if (result.Task == 2)
                            response = new DeleteResponse(request);
                        else
                            response = new Response(request);

                        Console.WriteLine("[{0}] Sending push update for GetAnchorRequest to client No.{1}, AnchorID {2}", DateTime.Now.ToString(), pushListener[i].Item2.GetClientNo(), result.AnchorId);

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


