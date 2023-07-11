using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Core.Events;
using WalletConnectSharp.Core.Events.Request;
using WalletConnectSharp.Core.Events.Response;
using WalletConnectSharp.Core.Models;
using WalletConnectSharp.Core.Network;
using NativeWebSocket;
using UnityEngine;
using System.Collections.Generic;

/**
 * Unity transport implementation using NativeWebSocket
 */
public class UnityTransport : ITransport
{
    private EventDelegator eventDelegator;
    private WebSocket client;
    private WebSocket nextClient;

    private Queue<NetworkMessage> transmitQueue = new Queue<NetworkMessage>();
    private HashSet<string> subscriptions = new HashSet<string>();

    private bool opened = false;
    private bool paused = false;

    public UnityTransport(EventDelegator eventDelegator)
    {
        this.eventDelegator = eventDelegator;
    }

    private void DispatchTransmitQueue()
    {
        while (transmitQueue.Count > 0)
        {
            var msg = transmitQueue.Peek();
            client.SendText(msg.Payload);
        }

        Debug.Log("[WCUnityTransport] Message queue dispatched");
    }

    public bool Connected => client != null && opened && client.State == WebSocketState.Open;

    public string URL { get; private set; }

    public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    public event EventHandler<MessageReceivedEventArgs> OpenReceived;

    public async Task Open(string bridgeURL, bool clearSubscriptions = true)
    {
        if (bridgeURL != URL || clearSubscriptions)
        {
            ClearSubscriptions();
        }

        URL = bridgeURL;

        await OpenSocket();
    }

    private async Task OpenSocket()
    {
        if (nextClient != null)
        {
            return;
        }

        var urlToConnect = URL;

        if (urlToConnect.StartsWith("https"))
        {
            urlToConnect = urlToConnect.Replace("https", "wss");
        }
        else if (urlToConnect.StartsWith("http"))
        {
            urlToConnect = urlToConnect.Replace("http", "ws");
        }

        nextClient = new WebSocket(urlToConnect);

        var openCompleted = new TaskCompletionSource<bool>();

        nextClient.OnMessage += OnMessageReceived;
        nextClient.OnError += (errStr) => HandleException(new Exception(errStr));

        nextClient.OnOpen += () =>
        {
            OnOpen();

            OpenReceived?.Invoke(this, null);

            openCompleted.SetResult(true);
        };

        Debug.Log($"[WCUnityTransport] Connecting WS to {URL}...");

        nextClient.Connect().ContinueWith(t => HandleException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);

        await openCompleted.Task;

        Debug.Log("[WCUnityTransport] WS Connected");
    }

    private void HandleException(Exception ex)
    {
        OnError(ex.ToString());
    }

    private async void OnOpen()
    {
        await Close();

        client = nextClient;
        nextClient = null;

        QueueSubscriptions();

        opened = true;

        DispatchTransmitQueue();
    }

    private async void TryReconnect(WebSocketCloseCode closeCode)
    {
        if (paused)
        {
            return;
        }

        nextClient = null;
        await OpenSocket();
    }

    public async Task Close()
    {
        Debug.Log("[WCUnityTransport] Closing WS");
        try
        {
            if (client != null)
            {
                opened = false;
                await client.Close();
                Debug.Log("[WCUnityTransport] WS closed");
            }
        }
        catch (Exception e)
        {
            Debug.Log("[WCUnityTransport] WS close exception: " + e.Message);
        }
    }

    private void QueueSubscriptions()
    {
        foreach (var topic in subscriptions)
        {
            transmitQueue.Enqueue(GenerateSubMessage(topic));
        }

        Debug.Log("[WCUnityTransport] Subscriptions queued");
    }

    private async void OnMessageReceived(byte[] bytes)
    {
        Debug.Log("[WCUnityTransport] WS Msg received");
        if (bytes.Length == 0)
        {
            Debug.LogError("[WCUnityTransport] Received empty message");
            return;
        }

        var jsonText = Encoding.UTF8.GetString(bytes);
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("[WCUnityTransport] Received empty JSON string");
            return;
        }

        try
        {
            var msg = JsonConvert.DeserializeObject<NetworkMessage>(jsonText);

            // Send ACK
            await SendMessage(new NetworkMessage()
            {
                Payload = "",
                Type = "ack",
                Silent = true,
                Topic = msg.Topic,
            });

            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(msg, this));
        }
        catch (Exception e)
        {
            Debug.Log("[WCUnityTransport] Exception " + e.Message);
        }
    }

    private async void OnError(string errorMsg)
    {
        Debug.LogError("[WCUnityTransport] WebSocket error: " + errorMsg);
    }

    public async Task SendMessage(NetworkMessage message)
    {
        Debug.Log("[WCUnityTransport] WS Sending msg");
        var msgJson = JsonConvert.SerializeObject(message);

        await client.SendText(msgJson);
    }

    private NetworkMessage GenerateSubMessage(string topic)
    {
        return new NetworkMessage()
        {
            Payload = "",
            Type = "sub",
            Silent = true,
            Topic = topic,
        };
    }

    public async Task Subscribe(string topic)
    {
        Debug.Log("[WCUnityTransport] WS subscribing");
        await SendMessage(GenerateSubMessage(topic));

        subscriptions.Add(topic);
    }

    public async Task Subscribe<T>(string topic, EventHandler<JsonRpcResponseEvent<T>> callback) where T : JsonRpcResponse
    {
        Debug.Log("WS Sub");
        await Subscribe(topic);

        eventDelegator.ListenFor(topic, callback);
    }

    public async Task Subscribe<T>(string topic, EventHandler<JsonRpcRequestEvent<T>> callback) where T : JsonRpcRequest
    {
        await Subscribe(topic);

        eventDelegator.ListenFor(topic, callback);
    }

    public void ClearSubscriptions()
    {
        Debug.Log("[WCUnityTransport] WS Clearing subscriptions");
        subscriptions.Clear();
        transmitQueue.Clear();
    }

    public void Dispose()
    {
        Debug.Log("[WCUnityTransport] WS Dispose");
        client?.CancelConnection();
    }

    public void DispatchSocketMessageQueue()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        client?.DispatchMessageQueue();
#endif
    }

    public async Task OnApplicationPause(bool pauseStatus)
    {
        Debug.Log($"[WCUnityTransport] WS app pause: {pauseStatus}");

        if (pauseStatus)
        {
            paused = true;

            await Close();
        }
        else if (paused)
        {
            await Open(URL, false);

            foreach (var topic in subscriptions)
            {
                await Subscribe(topic);
            }
        }
    }
}
