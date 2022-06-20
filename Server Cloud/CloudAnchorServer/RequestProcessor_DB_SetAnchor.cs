using System;
using System.Collections.Generic;
using NetworkCommon;
using DBCon;

namespace CloudAnchorServer
{
    public class AnchorResult : Object
    {
        internal int Task { get; private set; }
        internal string Context { get; private set; }
        internal string AnchorId { get; private set; }

        public AnchorResult(int task, string context, string anchorId) : base()
        {
            this.Task = task;
            this.Context = context;
            this.AnchorId = anchorId;
        }
    }

    class RequestProcessor_DB_SetAnchor : RequestProcessor_DB
    {
        public RequestProcessor_DB_SetAnchor(DataManager dataManager, PushRequestProcessor pushRequestProcessor) : base("setAnchor", dataManager, pushRequestProcessor) { }
        protected override void SendResponse(DataRequest request, HandleClient client)
        {
            Console.WriteLine("[{0}] Received SetAnchorRequest from client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            bool result = false;
            SetAnchorRequest r = (SetAnchorRequest)request;

            string context = r.AnchorContext;
            string anchorId = r.AnchorId;
            
            switch (r.Task)
            {
                case 0:
                    result = dataManager.InsertAnchor(context, anchorId);

                    if (result)
                        Console.WriteLine("Save -> anchor: {0}, context: {1}", anchorId, context);
                    else
                        Console.WriteLine("Unable to save -> anchor: {0}, context: {1}", anchorId, context);
                    break;
                case 1:
                    break;
                case 2:
                    result = dataManager.DeleteAnchor(context, anchorId);
                    if (result)
                    {
                        Console.WriteLine("Delete -> anchor: {0}, context: {1}", anchorId, context);
                    }
                    else
                        Console.WriteLine("Unable to delete -> anchor: {0}, context: {1}", anchorId, context);
                    break;
                default:
                    Console.WriteLine($"Unable to process task {r.Task} for Request setAnchor.");
                    break;
            }

            Console.WriteLine("[{0}] Sending response for SetAnchorRequest to client No.{1}", DateTime.Now.ToString(), client.GetClientNo());

            r.SetResult(result);
            client.SendResponse(new Response(r));

            if (result)
                this.PushRequestProcessor.Notify(new AnchorResult(r.Task, context, anchorId));
        }

        protected override bool IsHandleRequest(object r) { return r is SetAnchorRequest; }
    }
}
