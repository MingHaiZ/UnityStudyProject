using System.Net.Sockets;
using UnityEngine;

public class ClientSession
{
    public Socket Socket;
    public string PlayerId;
    public byte[] Buffer;
    public ClientSession(Socket socket)
    {
        Socket = socket;
        Buffer = new byte[1024];
        PlayerId = System.Guid.NewGuid().ToString().Substring(0, 8);
    }
}
