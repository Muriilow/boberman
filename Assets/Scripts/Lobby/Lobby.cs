using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using UnityEngine.Events;

public class Lobby : MonoBehaviour
{
    public static Lobby currentLobby;

    public UnityEvent OnLobbyCreated;
    public UnityEvent OnLobbyJoined;
    public UnityEvent OnLobbyLeave;
    
    public GameObject InLobbyFriend;
    public Transform content;
    
    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();

    private void Start()
    {
        DontDestroyOnLoad(this);
    }
}
