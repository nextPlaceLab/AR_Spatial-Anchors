using System;
using System.Collections.Generic;

namespace NetworkCommon
{

    [Serializable()]
    public class Request
    {
        public string RequestType { get; private set; }
        public long Id { get; private set; }

        private static long count = 0;
        private static object _lock = new object();
        public Request(string requestType)
        {
            this.RequestType = requestType;

            lock (_lock)
            {
                this.Id = count++;
            }
        }

        public override string ToString()
        {
            return String.Format("Request({0}): {1}", Id, RequestType);
        }
    }
    [Serializable]
    public class DataRequest : Request
    {
        public virtual bool HasResult { get; private set; }
        public DataRequest(string requestType) : base(requestType)
        {
        }
    }
    [Serializable]
    public class DataRequest<T> : DataRequest
    {
        public override bool HasResult { get { return Result != null; } }
        public T Result { get; private set; }
        public DataRequest(string requestType) : base(requestType) { }
        public void SetResult(T result)
        {
            Result = result;
        }
    }



    [Serializable]
    public class GetContextRequest : DataRequest<string[]>
    {
        public const string REQUEST_TYPE = "getContext";
        
        public GetContextRequest() : base(REQUEST_TYPE)
        {
            
        }
    }

    [Serializable]
    public class DeleteContextRequest : DataRequest<bool>
    {
        public const string REQUEST_TYPE = "deleteContext";
        public string AnchorContext { get; private set; }

        public DeleteContextRequest(string context) : base(REQUEST_TYPE)
        {
            AnchorContext = context;
        }
    }

    [Serializable]
    public class SetAnchorRequest : DataRequest<bool>
    {
        public const string REQUEST_TYPE = "setAnchor";
        public const int CREATE = 0;
        public const int UPDATE = 1;
        public const int DELETE = 2;
        public string AnchorId { get; private set; }
        public string AnchorContext { get; private set; }
        public int Task { get; private set; }
        public SetAnchorRequest(string anchorId, string context, int task = CREATE) : base(REQUEST_TYPE)
        {
            AnchorId = anchorId;
            AnchorContext = context;
            Task = task;
        }
        public override string ToString()
        {
            return base.ToString() + String.Format("task = {0}", Task);
        }
    }


    [Serializable]
    public class GetAnchorRequest : DataRequest<string[]>
    {
        public const string REQUEST_TYPE = "getAnchor";
        public string AnchorContext { get; private set; }
        public bool IsUpdateListener { get; private set; }
        public GetAnchorRequest(string context, bool isUpdateListener) : base(REQUEST_TYPE)
        {
            AnchorContext = context;
            IsUpdateListener = isUpdateListener;
        }
    }


    [Serializable]
    public class GetARModelRequest : DataRequest<ModelData[]>
    {
        public const string REQUEST_TYPE = "getARModel";
        public const int BY_ANCHOR = 0;
        public const int BY_CONTEXT = 1;
        public const int BOTH = 2;
        public string AnchorId { get; private set; }
        public string ModelContext { get; private set; }
        public int Task { get; private set; }
        public bool IsUpdateListener { get; private set; }
        public GetARModelRequest(string anchorId, int task = BY_ANCHOR, bool isUpdateListener = true, string modelContext = "") : base(REQUEST_TYPE)
        {
            AnchorId = anchorId;
            ModelContext = modelContext;
            IsUpdateListener = isUpdateListener;
            Task = task;
        }
    }


    [Serializable]
    public class SetARModelRequest : DataRequest<bool[]> // is an answer required?
    {
        public const string REQUEST_TYPE = "setARModel";
        public const int CREATE = 0;
        public const int UPDATE = 1; 
        public const int UPDATE_FINISHED = 2;
        public const int DELETE = 3;

        public ModelData[] Models { get; private set; }
        public int Task { get; private set; }
        public SetARModelRequest(ModelData[] models, int task = CREATE) : base(REQUEST_TYPE)
        {
            // modeldata => array! multiple pose syncs?
            Models = models;
            Task = task;
        }
        public override string ToString()
        {
            return base.ToString() + String.Format("task = {0}", Task);
        }
    }

}
