using System.Collections.Generic;
using UnityEngine;

public class PlayerManager
{
    private static PlayerManager _instance;
    private static readonly object Locker = new object();
    
    public static PlayerManager Instance
    {
        get
        {
            if (_instance==null)
            {
                lock (Locker)
                {
                    _instance = new  PlayerManager();  
                }
            }
            return _instance;
        }
    }
    
    public GameObject playerPrefab;
    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

    public void InitializePlayers(string playerId)
    {
        if (playerPrefab==null)
        {
            Debug.LogError("PlayerPrefab Is null!");
            return;
        }

        var gameObject = GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        players.Add(playerId, gameObject);
    }
    
}