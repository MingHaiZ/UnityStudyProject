using System;
using UnityEngine;
using Unity.Netcode;

namespace NetcodeForGameobjects
{
    public class HelloWorldManager : MonoBehaviour
    {
        private NetworkManager m_networkManager;

        private void Awake()
        {
            m_networkManager = GetComponent<NetworkManager>();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!m_networkManager.IsClient && !m_networkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        private void SubmitNewPosition()
        {
            if (GUILayout.Button(m_networkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (m_networkManager.IsServer && !m_networkManager.IsClient)
                {
                    foreach (ulong uid in m_networkManager.ConnectedClientsIds)
                    {
                        m_networkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>()
                            .Move();
                    }
                }
                else
                {
                    var playerObject = m_networkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }

        private void StatusLabels()
        {
            var mode = m_networkManager.IsHost ? "Host" : m_networkManager.IsServer ? "Server" : "Client";
            GUILayout.Label("Transport: " + m_networkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host"))
            {
                m_networkManager.StartHost();
            }

            if (GUILayout.Button("Server"))
            {
                m_networkManager.StartServer();
            }

            if (GUILayout.Button("Client"))
            {
                m_networkManager.StartClient();
            }
        }
    }
    
}