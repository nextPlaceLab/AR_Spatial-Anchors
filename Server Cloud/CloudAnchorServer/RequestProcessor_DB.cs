using System;
using System.Collections.Generic;
using DBCon;

/// <summary>
/// Summary description for RequestProcessor_DB
/// </summary>
namespace CloudAnchorServer
{
    class RequestProcessor_DB : RequestProcessor
    {
        internal DataManager dataManager { get; }
        internal PushRequestProcessor PushRequestProcessor;

        public RequestProcessor_DB(string requestType, DataManager dataManager, PushRequestProcessor pushRequestProcessor=null) : base(requestType)
        {
            this.dataManager = dataManager;
            this.PushRequestProcessor = pushRequestProcessor;
        }
    }
}
