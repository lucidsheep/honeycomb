#if !UNITY_WEBGL || UNITY_EDITOR

using System.Net.Sockets;
using System.Reflection;
using System;
using WebSocketSharp;
using UnityEngine;

public class WebSocketSharpWebSocketClient : IWebSocketClient
{
    private static readonly Lazy<WebSocketSharpWebSocketClient> Lazy = 
        new Lazy<WebSocketSharpWebSocketClient> (() => new WebSocketSharpWebSocketClient());

    public static WebSocketSharpWebSocketClient Instance => Lazy.Value;
    
    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> ReceivedByteArrayMessage;
    public event Action<string> ReceivedTextMessage;
    public event Action ReceivedError;
    public event Action<string> ReceivedLogMessage;

    public static LogLevel GlobalLogLevel = LogLevel.Fatal;

    public LogLevel logLevel { get { return _webSocketConnection.Log.Level; } set { _webSocketConnection.Log.Level = value; } }
    private WebSocket _webSocketConnection;
    
    public WebSocketSharpWebSocketClient() { }
    
    public void ConnectToServer(string address, int port, bool isUsingSecureConnection)
    {
        if (_webSocketConnection != null) return;

        var urlPrefix = isUsingSecureConnection ? "wss" : "ws";

        //_webSocketConnection = new WebSocket($"{urlPrefix}://{address}:{port}/Listener");
        _webSocketConnection = new WebSocket(urlPrefix + "://" + address);
        if(isUsingSecureConnection)
            _webSocketConnection.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        //_webSocketConnection = new WebSocket(urlPrefix + "://kq.style:8080/listener");

        _webSocketConnection.OnOpen += (sender, args) =>
        {
            Connected?.Invoke();
        };
        
        _webSocketConnection.OnClose += (sender, args) =>
        {
            Debug.Log(args.Code.ToString());
            _webSocketConnection = null;
            Disconnected?.Invoke();
        };
        
        _webSocketConnection.OnMessage += (sender, args) =>
        { 
            if (args.IsBinary)
            {
                ReceivedByteArrayMessage?.Invoke(args.RawData);
            } else if(args.IsText)
            {
                ReceivedTextMessage?.Invoke(args.Data);
            }
        };

        _webSocketConnection.OnError += (sender, args) =>
        {
            ReceivedError?.Invoke();
        };

        _webSocketConnection.Log.Level = GlobalLogLevel;
        _webSocketConnection.Log.Output += OnLog;
        _webSocketConnection.Connect();
        
        ConfigureNoDelay();
    }

    private void OnLog(LogData data, string msg)
    {
        //Debug.Log(data.Message + "," + msg);
        ReceivedLogMessage?.Invoke(data.Message + "," + msg);
    }
    public void DisconnectFromServer()
    {
        _webSocketConnection?.Close();
        _webSocketConnection = null;
    }

    public void SendMessageToServer(byte[] array, int size)
    {
        _webSocketConnection?.Send(array);
    }
    
    private void ConfigureNoDelay()
    {
        try
        {
            var field = typeof(WebSocket).GetField("_tcpClient", BindingFlags.NonPublic);
            if (field != null && field.GetValue(_webSocketConnection) is TcpClient tcpClient)
            {
                tcpClient.NoDelay = true;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Couldn't disable Nagle's algorithm error: {exception.Message}");
        }
    }
}
#endif