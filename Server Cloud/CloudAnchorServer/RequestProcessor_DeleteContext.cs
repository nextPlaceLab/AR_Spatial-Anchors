using System;
using NetworkCommon;
using DBCon;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 

namespace CloudAnchorServer
{
    class RequestProcessor_DeleteContext : RequestProcessor_DB
    {
        public RequestProcessor_DeleteContext(DataManager dataManager) : base("deleteContext", dataManager)
        {
        }

        protected override void SendResponse(DataRequest request, HandleClient client)
        {
            Console.WriteLine("[{0}] Received DeleteContextRequest from client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            DeleteContextRequest r = (DeleteContextRequest)request;

            bool result = dataManager.DeleteContext(r.AnchorContext);

            Console.WriteLine("[{0}] Sending response for DeleteContextRequest to client No.{1}, Context: {2} success: {3}", DateTime.Now.ToString(), client.GetClientNo(), r.AnchorContext, result);

            r.SetResult(result);
            client.SendResponse(new Response(r));
        }

        protected override bool IsHandleRequest(object r) { return r is DeleteContextRequest; }
    }
}
