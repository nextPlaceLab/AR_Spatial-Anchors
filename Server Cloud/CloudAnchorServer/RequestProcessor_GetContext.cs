using System;
using NetworkCommon;
using DBCon;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 

namespace CloudAnchorServer
{
    class RequestProcessor_GetContext : RequestProcessor_DB
    {
        public RequestProcessor_GetContext(DataManager dataManager) : base("getContext", dataManager)
        {
        }

        protected override void SendResponse(DataRequest request, HandleClient client)
        {
            Console.WriteLine("[{0}] Received GetContextRequest from client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            GetContextRequest r = (GetContextRequest)request;

            string[] result = dataManager.GetContexts();

            Console.WriteLine("Found {0} contexts: ", result.Length);
            foreach (string context in result)
                Console.WriteLine(context);

            Console.WriteLine("[{0}] Sending response for GetContextRequest to client No.{1}, #context: {2}", DateTime.Now.ToString(), client.GetClientNo(), result.Length);

            r.SetResult(result);
            client.SendResponse(new Response(r));
        }

        protected override bool IsHandleRequest(object r) { return r is GetContextRequest; }
    }
}
