using System;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private bool IsHost = false;
    [SerializeField] private int port = 8080;
    private Socket _socket;

    private void Awake()
    {
        if (IsHost)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        else
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
