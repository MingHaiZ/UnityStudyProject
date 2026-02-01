using System;
using Unity.Netcode;
using UnityEngine;

namespace NetcodeForGameobjects
{
    public class HelloWorldPlayer : MonoBehaviour
    {
        private NetworkObject m_networkManager;

        private void Start()
        {
            m_networkManager = GetComponent<NetworkObject>();
        }
        public void Move()
        {
            Debug.Log("Moveing");
        }
    }
}