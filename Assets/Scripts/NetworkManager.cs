using System;
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
    
    private Socket _socket;
    private byte[] _buffer;
    private Dictionary<string,ClientSession> _sockets;
    
    public static NetworkManager Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
        }
        
#if UNITY_EDITOR
        IsHost = CurrentPlayer.IsMainEditor;

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
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
            _socket.Listen(10);
            Debug.Log($"<color=green>服务器:</color> 正在端口 {port} 等待连接...");

            _socket.BeginAccept(OnClientConnected, null);
        }
        catch (Exception e)
        {
            Debug.LogError($"服务器启动失败: {e.Message}");
        }
    }

    private void OnClientConnected(IAsyncResult ar)
    {
        Socket client = _socket.EndAccept(ar);
        var clientSession = new ClientSession(client);
        lock (_sockets)
        {
            _sockets.Add(clientSession.PlayerId,clientSession);
        }
        Debug.Log("<color=green>服务器:</color> 检测到客户端连入！");

        client.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnDataReceived, client);
        _socket.BeginAccept(OnClientConnected, null);
        
    }

    private void OnDataReceived(IAsyncResult ar)
    {
        Socket clientSocket = ar.AsyncState as Socket;

        try
        {
            var byteRead = _socket.EndReceive(ar);
            if (byteRead > 0)
            {
                string msg = Encoding.UTF8.GetString(_buffer, 0, byteRead);
                Debug.Log($"<color=green>服务器收到消息:</color> {msg}");
            }
        }
        catch (Exception e)
        {
            Debug.Log($"客户端断开连接: {e.Message}");
        }
    }


    private void StartClient()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Debug.Log("<color=yellow>客户端:</color> 正在尝试连接服务器...");
        _socket.BeginConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port), OnConnectServer, null);
    }

    private void OnConnectServer(IAsyncResult ar)
    {
        try
        {
            _socket.EndConnect(ar);
            Debug.Log("<color=yellow>客户端:</color> 连接服务器成功！");

            string msg = " Hello World";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            _socket.Send(data);
            Debug.Log("<color=yellow>客户端:</color> 消息已发送。");
        }
        catch (Exception e)
        {
            Debug.LogError($"客户端连接失败: {e.Message}");
        }
    }

    private void Update()
    {
    }

    
    
    private void OnDestroy()
    {
        if (_socket != null)
        {
            _socket.Close();
        }
    }

    public void HandlerMessage(Vector2 readValue)
    {
        
    }
    
}