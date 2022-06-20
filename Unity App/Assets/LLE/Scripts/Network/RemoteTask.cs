
using System;
using NetworkCommon;

namespace LLE.Network
{
    public class RemoteTask
    {
        public Request Request { get; private set; }
        public Action<Response> Callback { get; private set; }
        public RemoteTask(Request request, Action<Response> callback)
        {
            Request = request;
            Callback = callback;
        }
    }
}


