
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Network.Client;
using NetworkCommon;

using System.Threading;
using UnityEngine.Events;

namespace LLE.Network
{
    public class LLEClient
    {
        public UnityEvent<bool> OnConnectionChanged = new UnityEvent<bool>();
        private bool isConnected => connector.IsConnected;

        private object _lock = new object();
        private Queue<RemoteTask> remoteTasks = new Queue<RemoteTask>();

        private Connector connector;
        private Dictionary<Request, Action<Response>> callbacks = new Dictionary<Request, Action<Response>>(new RequestComparer());
        private Task listener;

        public LLEClient(string serverIP, int port, byte[] certAsBytes)
        {
            connector = new Connector(serverIP, port, certAsBytes, r =>
             {
                 Debug.Log("LLEClient: Response -> " + r.ToString() + " on tid = " + Thread.CurrentThread.ManagedThreadId);
                 if (callbacks.ContainsKey(r.Request))
                 {
                     var callback = callbacks[r.Request];
                     Task.Run(() => callback.Invoke(r));
                 }
                 else
                 {
                     Debug.Log("LLEClient: callback not found -> " + r.Request);
                 }
             });
            connector.OnConnectionChanged.AddListener((isConnected) =>
            {
                OnConnectionChanged.Invoke(isConnected);
            });

            listener = Task.Run(ListenForRequest);
        }
        public bool EnsureConnection()
        {
            if (!isConnected)
                return connector.Connect();
            return isConnected;
        }
        public void QueueRequest(Request request, Action<Response> callback)
        {
            var task = new RemoteTask(request, callback);
            lock (_lock)
            {
                remoteTasks.Enqueue(task);
            }
        }
        private void ListenForRequest()
        {
            Debug.Log("Listen for requests on tid " + Thread.CurrentThread.ManagedThreadId);
            RemoteTask task;

            while (true)
            {

                if (isConnected)
                {
                    task = null;
                    lock (_lock)
                    {
                        if (remoteTasks.Count > 0)
                            task = remoteTasks.Dequeue();
                    }

                    if (task != null)
                        SendRequest(task.Request, task.Callback);
                }
            }
        }

        private void SendRequest(Request request, Action<Response> callback)
        {
            if (callbacks.ContainsKey(request))
            {
                Debug.Log("LLEClient: Request already send! -> reject");
                return;
            }

            callbacks.Add(request, callback);
            try
            {
                connector.SendRequest(request);
            }
            catch (Exception e)
            {

                Debug.Log("LLEClient: exception -> " + e.Message);
            }
        }

        internal void Close()
        {
            Debug.Log("LLEclient: close");
            connector.Close();
        }

        internal bool IsConnected()
        {
            return isConnected;
        }
    }
}


