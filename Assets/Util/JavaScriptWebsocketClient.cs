#if UNITY_WEBGL

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

//todo - need to implement non-instanced version of javascript WS
//in JS, hold WSs in arrays and use IDs to keep track of them internally
//in Unity all things received are static, but can send messages individually (?)

public class JavaScriptWebsocketClient : IWebSocketClient
{

    public event Action Connected;
    public event Action Disconnected;
    public event Action<byte[]> ReceivedByteArrayMessage;
    public event Action<string> ReceivedTextMessage;
    public event Action ReceivedError;
    public event Action<string> ReceivedLogMessage;
    
    private delegate void ByteArrayMessageCallback(int socketID, IntPtr buffer, IntPtr length);
    private delegate void TextMessageCallback(int socketID, string str);
    private delegate void LogMessageCallback(int socketID, string str);
    private delegate void SocketCallback(int socketID);

    [DllImport("__Internal")]
    private static extern void ConnectWebSocket(int socketID, string wsUri);
    
    [DllImport("__Internal")]
    private static extern void DisconnectWebSocket(int socketID);
    
    [DllImport("__Internal")]
    private static extern void SetupConnectionOpenCallbackFunction(int socketID, SocketCallback callback);
    
    [DllImport("__Internal")]
    private static extern void SetupConnectionClosedCallbackFunction(int socketID, SocketCallback action);
    
    [DllImport("__Internal")]
    private static extern void SetupReceivedByteArrayMessageCallbackFunction(int socketID, ByteArrayMessageCallback callback);

    [DllImport("__Internal")]
    private static extern void SetupReceivedTextMessageCallbackFunction(int socketID, TextMessageCallback callback);

    [DllImport("__Internal")]
    private static extern void SetupReceivedErrorCallbackFunction(int socketID, SocketCallback action);

    [DllImport("__Internal")]
    private static extern void SendByteArrayMessage(int socketID, byte[] array, int size);

    static int sidCounter = 0;
    int sid = 0;
    static List<JavaScriptWebsocketClient> socketList;

    public JavaScriptWebsocketClient()
    {
        sid = sidCounter;
        if (sid == 0) socketList = new List<JavaScriptWebsocketClient>();
        sidCounter++;
        socketList.Add(this);

        SetupConnectionOpenCallbackFunction(sid, ConnectionOpenCallback);
        SetupConnectionClosedCallbackFunction(sid, ConnectionClosedCallback);
        SetupReceivedByteArrayMessageCallbackFunction(sid, ReceivedByteArrayMessageCallback);
        SetupReceivedTextMessageCallbackFunction(sid, ReceivedTextMesssageCallback);
        SetupReceivedErrorCallbackFunction(sid, ReceivedErrorCallback);
    }

    public void ConnectToServer(string address, int port, bool isUsingSecureConnection)
    {
        var urlPrefix = isUsingSecureConnection ? "wss" : "ws";
        //ConnectWebSocket($"{urlPrefix}://{address}:{port}/listener");
        ConnectWebSocket(sid, urlPrefix + "://" + address);
    }
    
    public void DisconnectFromServer()
    {
        DisconnectWebSocket(sid);
    }

    public void SendMessageToServer(byte[] array, int size)
    {
        SendByteArrayMessage(sid, array, size);
    }
    
    [MonoPInvokeCallback(typeof(SocketCallback))]
    private static void ConnectionOpenCallback(int socketID)
    {
        socketList[socketID].Connected?.Invoke();
    }
    
    [MonoPInvokeCallback(typeof(SocketCallback))]
    private static void ConnectionClosedCallback(int socketID)
    {
        socketList[socketID].Disconnected?.Invoke();
    }
    
    [MonoPInvokeCallback(typeof(ByteArrayMessageCallback))]
    private static void ReceivedByteArrayMessageCallback(int socketID, IntPtr buffer, IntPtr length)
    {
        var readLength = length.ToInt32();
        var bytes = new byte[readLength];
        Marshal.Copy(buffer, bytes, 0, readLength);
        socketList[socketID].ReceivedByteArrayMessage?.Invoke(bytes);
        //Debug.Log("Received message: " + Convert.ToBase64String(bytes));
    }

    [MonoPInvokeCallback(typeof(TextMessageCallback))]
    private static void ReceivedTextMesssageCallback(int socketID, string str)
    {
        socketList[socketID].ReceivedTextMessage?.Invoke(str);
    }

    [MonoPInvokeCallback(typeof(LogMessageCallback))]
    private static void ReceivedLogMessageCallback(int socketID, string str)
    {
        socketList[socketID].ReceivedLogMessage?.Invoke(str);
    }

    [MonoPInvokeCallback(typeof(SocketCallback))]
    private static void ReceivedErrorCallback(int socketID)
    {
        Debug.Log("Received JS Error");
        socketList[socketID].ReceivedError?.Invoke();
    }
}
#endif
