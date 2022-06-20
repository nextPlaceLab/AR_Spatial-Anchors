using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCommon
{
    [Serializable]
    public class Response
    {
        public DataRequest Request { get; private set; }
        public long Id { get; private set; }
        public bool Success { get { return Request.HasResult; } }

        private static int count = 0;
        private static object _lock = new object();
        public Response(DataRequest request)
        {
            this.Request = request;

            lock (_lock)
            {
                this.Id = count++;
            }
        }
        public override string ToString()
        {
            return String.Format("Response({0}): request = {1}, success = {2}", Id, Request.Id, Success);
        }
    }

    [Serializable]
    public class DeleteResponse : Response
    {
        public DeleteResponse(DataRequest request) : base(request)
        {
            
        }
    }

}
