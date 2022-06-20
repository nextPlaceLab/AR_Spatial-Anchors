using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Generic;

/// <summary>
/// Summary description for SocketServer
/// </summary>
public class SocketServer
{
    private int clientCounter = 0;

    private X509Certificate serverCertificate = null;

    private TcpListener listener = null;
    private RequestHandlerServer requestHandler;

    public SocketServer(int port, RequestHandlerServer requestHandler)
    {

        Console.WriteLine("[{0}] Init SocketServer", DateTime.Now.ToString());

        // Set user certificate for securing connection
        this.serverCertificate = new X509Certificate("your_certificate.pfx", "your_key");

        this.listener = new TcpListener(IPAddress.Any, port);
        this.listener.Start(100); // <- max. number of clients

        this.requestHandler = requestHandler;

        Console.WriteLine("[{0}] Init SocketServer...done", DateTime.Now.ToString());
    }

    public X509Certificate GetCertificate()
    {
        return serverCertificate;
    }


    public void Listen()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Waiting connection ... ");
                TcpClient tcpclient = listener.AcceptTcpClient();

                clientCounter += 1;

                Console.WriteLine("[{0}] >> Client No: {1} started!", DateTime.Now.ToString(), Convert.ToString(clientCounter));

                HandleClient client = new HandleClient(this, requestHandler);
           
                client.StartClient(tcpclient, Convert.ToString(clientCounter));
            }
            catch (AuthenticationException ae)
            {
                Console.WriteLine(ae.ToString());
                Console.WriteLine("[{0}] >> Client No: {1} was unable to authenticate!", DateTime.Now.ToString(), Convert.ToString(clientCounter));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
