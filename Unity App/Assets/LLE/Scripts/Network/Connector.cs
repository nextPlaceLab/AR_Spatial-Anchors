using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;
using NetworkCommon;
using System.IO;
using UnityEngine.Events;


namespace Network.Client
{

    public class Connector
    {
        private string serverIP;
        private int port;

        private Action<Response> responseCallback;

        private TcpClient client = null;
        private SslStream sslStream;
        private X509Certificate2Collection certs;

        public bool IsConnected { get; private set; }
        public UnityEvent<bool> OnConnectionChanged = new UnityEvent<bool>();
        private bool isRunning;

        public Connector(string serverIP, int port, byte[] certAsBytes, Action<Response> responseCallback)
        {
            Debug.Log("Init Connector");

            X509Certificate2 clientCertificate = new X509Certificate2(certAsBytes, "livinglabessigfabrik123");
            //X509Certificate clientCertificate = new X509Certificate("Assets/Resources/privatekey.pfx", "livinglabessigfabrik123");
            certs = new X509Certificate2Collection(new X509Certificate2[] { clientCertificate });

            this.serverIP = serverIP;
            this.port = port;

            this.responseCallback = responseCallback;
        }


        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Debug.LogError("Certificate error: " + sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers. 
            return true;
        }

        public bool Connect()
        {
            Debug.Log("Connector: connect");
            try
            {
                client = new TcpClient(serverIP, port);

                sslStream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate),
                    null
                );

                sslStream.AuthenticateAsClient("localhost", certs, System.Security.Authentication.SslProtocols.Tls12, false);
                if (sslStream.IsAuthenticated)
                {
                    isRunning = true;
                    Thread ctThread = new Thread(WaitForData);
                    ctThread.Start();
                }
                else
                {
                    Debug.Log("Could not authenticate as client");
                }

                IsConnected = true;
                

            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                IsConnected = false;
            }
            OnConnectionChanged.Invoke(IsConnected);
            return IsConnected;
        }


        public bool SendRequest(Request request)
        {
            if (!IsConnected && !Connect())
            {
                Debug.Log("Connection error: re-connect failed");                
                return false;
            }

            byte[] brequest = BinarySerializer.Serialize(request);
            int sizeOfByteMessage = brequest.Length;

            sslStream.Write(brequest);
            sslStream.Flush();

            Debug.Log("Send request: " + request);
            return true;
        }

        internal void Close()
        {
            Debug.Log("Connector: close connection");
            isRunning = false;
			if (sslStream != null)
				sslStream.Close();
        }

        private void WaitForData()
        {
            while (isRunning)
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

                    //Debug.Log("Size of complete data: " + data.ToArray().Length);
                    Response response = (Response) BinarySerializer.Deserialize(data.ToArray());                    
                    responseCallback.Invoke(response);
                }
                catch (Exception e)
                {
                    // ToDo: emit to app that connection hast been disconnected
                    IsConnected = false;
                    OnConnectionChanged.Invoke(IsConnected);
                    Debug.LogError(e.ToString());
                    Debug.LogError("Connection to server is disconnected");
                    this.sslStream.Close();
                    this.client.Close();
                    break;
                }
            }
        }
    }
}
