using LLE.ASA;
using LLE.Network;
using LLE.Rx;
using LLE.Unity;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class LLEMain : MonoBehaviour
{
    public static string CONTEXT_EF = "essigfabrik";
    [SerializeField]
    private AzureSessionManager AzureSessionManager;    
    
    [SerializeField]
    private bool useLocalServer = false;
    [SerializeField]    
    private string localIP = "192.168.0.128";
    [SerializeField]
    private string serverIP = "85.214.213.66";
    [SerializeField]
    private int port = 11112;

    public UnityEvent OnStartEvent = new UnityEvent();

    private bool mIsConnected;
    private bool mIsAzureReady;

    public static LLEMain currentInstance { get; private set; }
    public LLEClient Client { get; private set; }

    private bool isTryConnect;

    void Awake()
    {
        Debug.Log("LLEmain: awake");
        currentInstance = this;

        // dummy to init dispatcher
        Dispatcher.ProcessOnUI(() => Debug.Log("Init Dispatcher"));

        // init fileIO
        FileIO.InitHomeOnUiThread();

        var certAsBytes = Resources.Load<TextAsset>("privatekey").bytes;
        isTryConnect = true;

        Task.Run(() =>
        {

            Client = new LLEClient(useLocalServer ? localIP : serverIP, port, certAsBytes);

            Client.OnConnectionChanged.AddListener((isConnected) =>
            {
                Debug.Log("LLEmain: invoke event -> client connected = " + isConnected);
                mIsConnected = isConnected;
                CheckStart();
            });
            AzureSessionManager.OnSessionReady.AddListener((isReady) =>
            {
                Debug.Log("LLEmain: invoke event -> onSessionReady = " + isReady);
                mIsAzureReady = isReady;
                CheckStart();
            });

            Debug.Log("LLEmain ready -> tid = " + Thread.CurrentThread.ManagedThreadId);
            while (!Client.IsConnected() && isTryConnect)
            {
                Debug.Log("LLEmain try to connect");
                Client.EnsureConnection();
                Thread.Sleep(100);
            }
        });
    }

    private void CheckStart()
    {
        Debug.LogFormat("Check start: isConnected = {0}, isAzrueReady = {1}", mIsConnected, mIsAzureReady);
        if (mIsConnected && mIsAzureReady)
            OnStartEvent.Invoke();
    }


    private void OnApplicationQuit()
    {
        Debug.Log("LLEmain: quit");
        isTryConnect = false;
        Client.Close();
    }

}
