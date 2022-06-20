using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CloudAnchorServer;
using DBCon;


namespace Server
{

    class Program
    {


        // Main Method
        static void Main(string[] args)
        {
            new Server();
        }


    }
    class Server
    {
        SocketServer socketServer;

        public Server()
        {
            DataManager dataManager = new DataManager();

            var pushAnchorProcessor = new PushRequestProcessor_DB_GetAnchors(dataManager);
            var pushModelProcessor = new PushRequestProcessor_DB_GetModels(dataManager);

            var setAnchorProcessor = new RequestProcessor_DB_SetAnchor(dataManager, pushAnchorProcessor);
            var getAnchorProcessor = new RequestProcessor_DB_GetAnchors(dataManager, pushAnchorProcessor);
            var setModelProcessor = new RequestProcessor_DB_SetModels(dataManager, pushModelProcessor);
            var getModelProcessor = new RequestProcessor_DB_GetModels(dataManager, pushModelProcessor);

            var getContextProcessor = new RequestProcessor_GetContext(dataManager);
            var deleteContextProcessor = new RequestProcessor_DeleteContext(dataManager);
            //var artProcessor = new ArtGalProcessor(anchorProcessor);
            List<RequestProcessor> procList = new List<RequestProcessor>
            {
                setAnchorProcessor,
                getAnchorProcessor,
                setModelProcessor,
                getModelProcessor,
                getContextProcessor,
                deleteContextProcessor,
                //artProcessor,
                //new RequestProcessor("test")

            };

            //RequestHandlerServer requestHandler = new RequestHandlerServer(RequestProcessor.getDemoProcessor());
            RequestHandlerServer requestHandler = new RequestHandlerServer(procList);

            socketServer = new SocketServer(11112,requestHandler);            
            // NOTE: listens on the current thread
            socketServer.Listen();
        }

    }
}
