using System;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using UnityEngine;
using Unity.Netcode;
public class ManageRounds : NetworkBehaviour
{
    private HashSet<ulong> _playersAliveIds = new HashSet<ulong>();
    
    private static bool _alreadyInitialised = false;
    public static ManageRounds Instance { get; private set; }
    public event Action OnGameOver;
    public int Round { get; private set; }
    public int MaxRounds { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Round = 1;
        MaxRounds = 3;
        DontDestroyOnLoad(gameObject);
    }
    
    #region Starting the Game 
    private void Start()
    {
        Debug.Log($"[ManageRounds] Start called on {gameObject.name} (Instance ID: {gameObject.GetInstanceID()})");

        if (_alreadyInitialised)
        {
            Debug.LogWarning($"[ManageRounds] Instance {gameObject.GetInstanceID()} is a duplicate. Destroying.");
            Destroy(gameObject); 
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.Log("NetworkManager already running. Marking as initialised.");
            _alreadyInitialised = true;
            return;
        }

        if (SteamLobby.Instance != null && SteamLobby.currentLobby.Id.Value != 0)
        {
            if (SteamLobby.currentLobby.Owner.Id == SteamClient.SteamId)
            {
                _alreadyInitialised = true;
                NetworkManager.Singleton.StartHost();
                Debug.Log("Starting host automatically (Steam Owner)");
            }
            else
            {
                var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
                if (transport != null)
                {
                    transport.targetSteamId = SteamLobby.currentLobby.Owner.Id;
                    Debug.Log($"Targeting Steam Host ID: {transport.targetSteamId}");
                }
                _alreadyInitialised = true;
                NetworkManager.Singleton.StartClient();
                Debug.Log("Starting client automatically (Steam Member)");
            }
        }
        else
        {
            Debug.LogWarning("No active Steam Lobby. Defaulting to StartHost for local testing.");
            _alreadyInitialised = true;
            NetworkManager.Singleton.StartHost();
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playersAliveIds.Clear();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                _playersAliveIds.Add(client.ClientId);
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        if (!IsHost)
            return;
        
        Debug.Log($"Starting game. Numbers of players active: {_playersAliveIds.Count}");
        ManageDrops.Instance.CreateTiles();
        ManageDrops.Instance.CreateWalls();
    }

    private void OnClientConnected(ulong clientId)
    {
        if(_playersAliveIds.Add(clientId))
            Debug.Log($"Player {clientId} connected. Total alive: {_playersAliveIds.Count}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (_playersAliveIds.Add(clientId))
        {
            Debug.Log($"Player {clientId} disconnected. Total alive: {_playersAliveIds.Count}");
            CheckWinCondition();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    #endregion 
    
    #region GameplayLoop 

    [Rpc(SendTo.Server)]
    public void PlayerDiedServerRpc(RpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (_playersAliveIds.Remove(clientId))
        {
            Debug.Log($"Player reported death. Total alive: {_playersAliveIds.Count}");
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (!IsServer) return;

        if (_playersAliveIds.Count <= 1)
        {
            Debug.Log("Game Over detected by server. Returning to Lobby.");
            EndGameClientRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameClientRpc()
    {
        if (!IsServer) 
            return;
        OnGameOver?.Invoke();
        // Reseta para a próxima partida
        _alreadyInitialised = false;
        Round++;
        //NetworkManager.SceneManager.LoadScene("Lobby", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    #endregion
}
