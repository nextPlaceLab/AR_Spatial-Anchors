using System;
using System.Collections.Generic;
using NetworkCommon;

namespace CloudAnchorServer
{
    public interface IRequestProcessor
    {
        void OnRequest(DataRequest r, HandleClient client);
    }



    // how about generics?!
    public class RequestProcessor : IRequestProcessor
    {
        private Object data;
        private string requestType;

        internal static List<RequestProcessor> getDemoProcessor()
        {
            return new List<RequestProcessor>
            {
                new RequestProcessor("test")
            };

        }

        public RequestProcessor(string requestType)
        {
            this.requestType = requestType;

            data = new SerializableData();
        }
        public virtual void OnRequest(DataRequest request, HandleClient client)
        {
            if (IsHandleRequest(request))
                SendResponse(request, client);
        }
        protected virtual void SendResponse(DataRequest request, HandleClient client)
        {
            Response response = new Response(request);
            client.SendResponse(response);
        }

        protected virtual Object GetData(Request request)
        {
            return data;
        }

        protected virtual bool IsHandleRequest(object r) { return r is Request; }

    }
    class PushRequestProcessor : RequestProcessor
    {
        protected List<Tuple<DataRequest, HandleClient>> pushListener = new List<Tuple<DataRequest, HandleClient>>();

        public PushRequestProcessor(string requestType) : base(requestType) { }
        public override void OnRequest(DataRequest request, HandleClient client)
        {
            if (IsHandleRequest(request))
                pushListener.Add(new Tuple<DataRequest, HandleClient>(request, client));
        }

        public virtual void Notify(Object data)
        {
            foreach (var listener in pushListener)
            {
                Response response = new Response(listener.Item1);
                listener.Item2.SendResponse(response);
            }
        }
    }
}
