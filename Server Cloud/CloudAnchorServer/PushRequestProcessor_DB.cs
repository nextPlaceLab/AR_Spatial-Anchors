using System;
using DBCon;
using NetworkCommon;

/// <summary>
/// Summary description for PushRequestProcessor_DB
/// </summary>
/// 
namespace CloudAnchorServer
{
    class PushRequestProcessor_DB : PushRequestProcessor
    {
        protected DataManager dataManager;

        public PushRequestProcessor_DB(string requestType, DataManager dataManager) : base(requestType)
        {
            this.dataManager = dataManager;
        }
    }
}
