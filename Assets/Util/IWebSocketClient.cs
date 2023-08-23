using System;
using System.Net;

public interface IWebSocketClient
{
    event Action Connected;
    event Action Disconnected;
    event Action<byte[]> ReceivedByteArrayMessage;
    event Action<string> ReceivedTextMessage;
    event Action ReceivedError;
    //not implemented in JS websocket
    event Action<string> ReceivedLogMessage;


    void ConnectToServer(string address, int port, bool isUsingSecureConnection);
    void DisconnectFromServer();
    void SendMessageToServer(byte[] array, int size);
}
