using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NUnit.Framework;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using Object = System.Object;

public class NetworkManager : MonoBehaviour
{
    public bool IsHost = false;
    [SerializeField] private int port = 8080;

    private Socket _serverListener;
    private Socket _clientSocket;

    private ConcurrentQueue<string> _mainThreadLogs = new ConcurrentQueue<string>();

    private byte[] _clientBuffer = new byte[1024];
    private Dictionary<string, ClientSession> _sockets = new Dictionary<string, ClientSession>();

    public static NetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

#if UNITY_EDITOR
        IsHost = CurrentPlayer.IsMainEditor;
        Application.runInBackground = true;
        Debug.Log($"[系统生成] 当前实例是否为主窗口: {CurrentPlayer.IsMainEditor}，分配角色: {(IsHost ? "服务器" : "客户端")}");
#endif
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsHost)
        {
            StartServer();
        }
        else
        {
            Invoke(nameof(StartClient), 1.0f);
        }
    }

    private void StartServer()
    {
        try
        {
            _serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _serverListener.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            _serverListener.Listen(10);
            Debug.Log($"<color=green>服务器:</color> 正在端口 {port} 等待连接...");

            _serverListener.BeginAccept(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"服务器启动失败: {e.Message}");
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        try
        {
            Socket client = _serverListener.EndAccept(ar);
            var clientSession = new ClientSession(client);
            lock (_sockets)
            {
                _sockets.Add(clientSession.PlayerId, clientSession);
            }

            Debug.Log($"<color=green>服务器:</color> 检测到客户端{clientSession.PlayerId}连入！");

            client.BeginReceive(clientSession.Buffer, 0, clientSession.Buffer.Length, SocketFlags.None, OnDataReceived,
                clientSession);
            _serverListener.BeginAccept(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.Log($"连接异常: {e.Message}");
        }
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        var session = ar.AsyncState as ClientSession;
        if (session == null || !session.Socket.Connected) return;

        try
        {
            var byteRead = session.Socket.EndReceive(ar);
            if (byteRead > 0)
            {
                string msg = Encoding.UTF8.GetString(session.Buffer, 0, byteRead);
                Debug.Log($"<color=green>服务器收到消息:</color> {msg}");
                session.Socket.BeginReceive(session.Buffer, 0, session.Buffer.Length, SocketFlags.None, OnDataReceived,
                    session);
            }
        }
        catch (Exception e)
        {
            CloseSession(session);
            Debug.Log($"客户端断开连接: {e.Message}");
        }
    }

    private void CloseSession(ClientSession session)
    {
        lock (_sockets)
        {
            if (_sockets.ContainsKey(session.PlayerId))
                _sockets.Remove(session.PlayerId);
        }

        session.Socket.Close();
        _mainThreadLogs.Enqueue($"玩家 {session.PlayerId} 断开连接");
    }


    private void StartClient()
    {
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("<color=yellow>客户端:</color> 正在尝试连接服务器...");
        _clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port), OnConnectServer, null);
    }

    private void OnConnectServer(IAsyncResult ar)
    {
        try
        {
            _clientSocket.EndConnect(ar);
            Debug.Log("<color=yellow>客户端:</color> 连接服务器成功！");

            SendString("Hello World");
            _clientSocket.BeginReceive(_clientBuffer, 0, _clientBuffer.Length, SocketFlags.None, OnClientDataReceived,
                null);
        }
        catch (Exception e)
        {
            Debug.LogError($"客户端连接失败: {e.Message}");
        }
    }

    private void OnClientDataReceived(IAsyncResult ar)
    {
        try
        {
            var endReceive = _clientSocket.EndReceive(ar);

            if (endReceive > 0)
            {
                var data = Encoding.UTF8.GetString(_clientBuffer, 0, endReceive);
                _mainThreadLogs.Enqueue($"<color=yellow>客户端收到广播:</color> {data}");
                _clientSocket.BeginReceive(_clientBuffer, 0, _clientBuffer.Length, SocketFlags.None,
                    OnClientDataReceived,
                    null);
            }

            if (endReceive == 0)
            {
                _mainThreadLogs.Enqueue("<color=yellow>客户端:</color> 服务器关闭了连接");
                _clientSocket.Close();
            }
        }
        catch (Exception e)
        {
            Debug.Log($"客户端接收掉线: {e.Message}");
        }
    }

    private void SendString(string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        _clientSocket.Send(data);
        Debug.Log("<color=yellow>客户端:</color> 消息已发送。");
    }


    private void Update()
    {
        while (_mainThreadLogs.TryDequeue(out string log))
        {
            Debug.Log(log);
        }
    }


    private void OnDestroy()
    {
        if (_serverListener != null)
        {
            _serverListener.Close();
        }

        if (_clientSocket != null)
        {
            _clientSocket.Close();
        }
    }

    public void HandleMessage(Vector2 inputDir)
    {
        if (IsHost)
        {
            return;
        }

        string msg = $"{inputDir.x:F2},{inputDir.y:F2}";
        SendString(msg);
        Debug.Log($"客户端发送指令: {msg}");
    }

    public void BroadcastMessage(Vector2 moveData)
    {
        if (!IsHost)
        {
            return;
        }

        string msg = $"MOVE:{moveData.x:F2},{moveData.y:F2}";
        var bytes = Encoding.UTF8.GetBytes(msg);
        lock (_sockets)
        {
            foreach (var client in _sockets.Values)
            {
                if (client.Socket != null && client.Socket.Connected)
                {
                    try
                    {
                        client.Socket.Send(bytes, 0, bytes.Length, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
        }

        _mainThreadLogs.Enqueue($"服务器已广播移动数据: {msg}");
    }
}