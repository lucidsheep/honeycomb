using UnityEngine;
using System.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using Unity.Jobs;
public class NetworkConnection
{

    static List<string> networkURLs = new List<string>();
    static List<IWebSocketClient> clients = new List<IWebSocketClient>();
    static int nextID = 0;

    public IWebSocketClient socketClient;

    public bool isConnected = false, isConnecting = false;
    public WebSocketSharp.LogLevel networkLogLevel;

    public string serverAddress;
    public int serverPort;

    bool useSecureSockets = true;

    public GameEvent gameEventDispatcher = new GameEvent();
    public UnityEvent<HMMatchState> tournamentEventDispatcher = new UnityEvent<HMMatchState>();
    public UnityEvent<string, GameEventData> rawEventDispatcher = new UnityEvent<string, GameEventData>();
    public UnityEvent<int> onGameID = new UnityEvent<int>();

    public Queue<string> eventQueue = new Queue<string>();
    public Queue<string> errorQueue = new Queue<string>();

    bool connectionEventFlag = false;

    public UnityEvent<bool> OnConnectionEvent = new UnityEvent<bool>();
    public UnityEvent<string> OnNetworkEvent = new UnityEvent<string>();
    public UnityEvent<string> OnNetworkErrorEvent = new UnityEvent<string>();

    LSTimer reconnectTimer;

    public struct ConnectionJob : IJob
    {
        int jobID;
        int port;
        bool useSecureConnection;

        public ConnectionJob(IWebSocketClient socket, string url, int wsPort, bool secureSocket = true)
        {
            jobID = nextID;
            nextID++;
            networkURLs.Add(url);
            port = wsPort;
            useSecureConnection = secureSocket;
            clients.Add(socket);
        }
        public void Execute()
        {
            clients[jobID].ConnectToServer(networkURLs[jobID], port, useSecureConnection);
        }
    }

    public ConnectionJob connection;

    public NetworkConnection(string url, int wsPort, bool secureSocket = true)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        socketClient = new JavaScriptWebsocketClient();
#else
        socketClient = new WebSocketSharpWebSocketClient();
#endif
        socketClient.Connected += OnConnected;
        socketClient.Disconnected += OnDisconnected;
        socketClient.ReceivedByteArrayMessage += OnReceivedByteArrayMessage;
        socketClient.ReceivedTextMessage += OnReceivedTextMessage;
        socketClient.ReceivedError += OnReceivedError;
        //socketClient.ReceivedLogMessage += OnReceivedLog;
        serverAddress = url;
        serverPort = wsPort;
        useSecureSockets = secureSocket;

        new LSTimer(.03f, Update, 0, -1);
    }

    private void OnConnected()
    {
        isConnected = true;
        isConnecting = false;
        connectionEventFlag = true;
        //(socketClient as WebSocketSharpWebSocketClient).logLevel = WebSocketSharp.LogLevel.Debug;
    }

    private void OnDisconnected()
    {
        isConnected = false;
        isConnecting = false;
        connectionEventFlag = true;
        reconnectTimer = new LSTimer(5f, StartConnection);
    }


    private void OnReceivedByteArrayMessage(byte[] bytes)
    {
        var json = System.Text.Encoding.UTF8.GetString(bytes); //Convert.ToBase64String(bytes);
        eventQueue.Enqueue(json);
    }

    private void OnReceivedTextMessage(string message)
    {
        eventQueue.Enqueue(message);

    }

    private void OnReceivedError()
    {
        errorQueue.Enqueue("Error");
    }

    private void OnReceivedLog(string msg)
    {
        errorQueue.Enqueue("Log Message: " + msg);
    }

    public void SendMessageToServer(string message)
    {
        if (!isConnected) return;

        var bArr = System.Text.Encoding.UTF8.GetBytes(message); //Convert.FromBase64String(json);
        var len = System.Text.Encoding.UTF8.GetByteCount(message);
        socketClient.SendMessageToServer(bArr, len);
    }

    private void SendMessageToServer(object obj)
    {
        SendMessageToServer(JsonUtility.ToJson(obj));
    }

    public void StartConnection()
    {
        if (isConnected || isConnecting) return;
        isConnecting = true;

        if (reconnectTimer != null)
            reconnectTimer.CancelTimer();

        connection = new ConnectionJob(socketClient, serverAddress, serverPort, useSecureSockets);
        connection.Schedule();

    }
    private void Update()
    {
        while(eventQueue.Count > 0)
        {
            OnNetworkEvent.Invoke(eventQueue.Dequeue());
        }
        while(errorQueue.Count > 0)
        {
            OnNetworkErrorEvent.Invoke(errorQueue.Dequeue());
        }
        if(connectionEventFlag)
        {
            connectionEventFlag = false;
            OnConnectionEvent.Invoke(isConnected);
        }
    }
}

