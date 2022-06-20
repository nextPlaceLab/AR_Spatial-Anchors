using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using CloudAnchorServer;
using NetworkCommon;
using Network.Server;

public class HandleClient
{
    TcpClient tcpclient;
    SslStream sslStream;
    private string clNo;
    private SocketServer socketServer;
    private RequestHandlerServer requestHandler;

    public HandleClient(SocketServer socketServer, RequestHandlerServer requestHandler)
    {
        this.socketServer = socketServer;
        this.requestHandler = requestHandler;
    }

    public string GetClientNo()
    {
        return clNo;
    }

    public void StartClient(TcpClient client, string clientNo)
    {
        tcpclient = client;
        clNo = clientNo;

        sslStream = new SslStream(
                tcpclient.GetStream(), false);
        sslStream.AuthenticateAsServer(socketServer.GetCertificate(), false, System.Security.Authentication.SslProtocols.Tls12, true);

        // Display the properties and settings for the authenticated stream.
        DisplaySecurityLevel(sslStream);
        DisplaySecurityServices(sslStream);
        DisplayCertificateInformation(sslStream);
        DisplayStreamProperties(sslStream);

        Thread ctThread = new Thread(WaitForData);
        ctThread.Start();
    }

    private void WaitForData()
    {
        while (true)
        {
            try
            {
                int inputBytes;
                IEnumerable<byte> data = Enumerable.Empty<byte>();

                do
                {
                    byte[] inputBuffer = new byte[1024];
                    inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                    Array.Resize<byte>(ref inputBuffer, inputBytes);
                    data = data.Concat(inputBuffer);
                } while (inputBytes == 1024);

                Console.WriteLine("Size of complete data: {0}", data.ToArray().Length);

                if (data.ToArray().Length == 0)
                {
                    // Client disconnected
                    throw new SocketException();
                }

                Console.WriteLine("Now, the RequestHandler should be called...");

                DataRequest request = (DataRequest)BinarySerializer.Deserialize(data.ToArray());
                requestHandler.HandleRequest(request, this);


            }
            catch (SocketException e)
            {
                Console.WriteLine(e.ToString());
                Console.WriteLine("Client {0} disconnected.", clNo);
                break;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: {0}", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                break;
            }
        }
    }

    internal void SendResponse(Response response)
    {
        Console.WriteLine("Sending response to client {0}", clNo);
        byte[] barray = BinarySerializer.Serialize(response);

        //int sizeOfByteMessage = Buffer.ByteLength(barray);
        int sizeOfByteMessage = barray.Length;

        Console.WriteLine("Size: {0}", sizeOfByteMessage);
        
        sslStream.Write(barray);
        sslStream.Flush();
    }


    private void DisplaySecurityLevel(SslStream stream)
    {
        Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
        Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
        Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
        Console.WriteLine("Protocol: {0}", stream.SslProtocol);
    }
    private void DisplaySecurityServices(SslStream stream)
    {
        Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
        Console.WriteLine("IsSigned: {0}", stream.IsSigned);
        Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
    }
    private void DisplayStreamProperties(SslStream stream)
    {
        Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
        Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
    }
    private void DisplayCertificateInformation(SslStream stream)
    {
        Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

        X509Certificate localCertificate = stream.LocalCertificate;
        if (stream.LocalCertificate != null)
        {
            Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                localCertificate.Subject,
                localCertificate.GetEffectiveDateString(),
                localCertificate.GetExpirationDateString());
        }
        else
        {
            Console.WriteLine("Local certificate is null.");
        }
        // Display the properties of the client's certificate.
        X509Certificate remoteCertificate = stream.RemoteCertificate;
        if (stream.RemoteCertificate != null)
        {
            Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                remoteCertificate.Subject,
                remoteCertificate.GetEffectiveDateString(),
                remoteCertificate.GetExpirationDateString());
        }
        else
        {
            Console.WriteLine("Remote certificate is null.");
        }
    }

}
