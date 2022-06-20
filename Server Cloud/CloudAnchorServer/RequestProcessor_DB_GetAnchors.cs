using System;
using System.Collections.Generic;
using DBCon;
using NetworkCommon;

/// <summary>
/// Summary description for RequestProcessor_DB_GetAnchors
/// </summary>
/// 

namespace CloudAnchorServer
{
    class RequestProcessor_DB_GetAnchors : RequestProcessor_DB
    {
        public RequestProcessor_DB_GetAnchors(DataManager dataManager, PushRequestProcessor_DB_GetAnchors pushRequestProcessor) : base("getAnchor", dataManager, pushRequestProcessor)
        {
        }

        public override void OnRequest(DataRequest request, HandleClient client)
        {
            if (IsHandleRequest(request))
            {
                GetAnchorRequest r = (GetAnchorRequest)request;

                Console.WriteLine("Received GetAnchorRequest from client No.{0} -> context = {1}", client.GetClientNo(), r.AnchorContext);

                string[] anchors = this.dataManager.GetAnchorsByContext(r.AnchorContext);
                r.SetResult(anchors);

                if (r.IsUpdateListener)
                {
                    Console.WriteLine("Added client No.{0} to getAnchor-PushListener", client.GetClientNo());
                    this.PushRequestProcessor.OnRequest(r, client);
                }

                Console.WriteLine("Sending response for GetAnchorRequest to client No.{0}, #anchor {1}", client.GetClientNo(), anchors.Length);

                Response response = new Response(r);
                client.SendResponse(response);
            }
        }
        protected override bool IsHandleRequest(object r) { return r is GetAnchorRequest; }
    }
}
