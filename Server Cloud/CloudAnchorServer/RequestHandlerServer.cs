using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudAnchorServer;
using NetworkCommon;


public class RequestHandlerServer
{

    private List<RequestProcessor> processors;

    public RequestHandlerServer(List<RequestProcessor> processors)
    {
        this.processors = processors != null ? processors : new List<RequestProcessor>();
    }
    public void HandleRequest(DataRequest req, HandleClient client = null)
    {
        foreach (var processor in this.processors)
        {
            processor.OnRequest(req, client);
        }
    }
}
