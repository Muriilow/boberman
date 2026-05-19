using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Steamworks;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ManageRounds : NetworkBehaviour
{
    private HashSet<ulong> _playersAliveIds = new HashSet<ulong>();
    
    private static bool _alreadyInitialised = false;
    public static ManageRounds Instance { get; private set; }
    public event Action OnGameOver;
    public int Round { get; private set; }
    public int MaxRounds { get; private set; }
    [SerializeField] private GameObject _playerPrefab;

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
        base.OnNetworkSpawn();
        if (!IsHost)
            return;
        
        _playersAliveIds.Clear();
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            _playersAliveIds.Add(client.ClientId);
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;

        
        Debug.Log($"Starting game. Numbers of players active: {_playersAliveIds.Count}");
        ManageDrops.Instance.CreateInfo();
        ManageDrops.Instance.CreateTiles();
        ManageDrops.Instance.CreateWalls();
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "Main" || !IsServer)
            return;

        Debug.Log("Main scene loaded. Initializing players and map for the round.");
        
        _alreadyInitialised = true;
        _playersAliveIds.Clear();
        
        foreach(var clientId in clientsCompleted)
        {
            _playersAliveIds.Add(clientId);

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            {
                // If the player object already exists, we don't spawn a new one.
                // This prevents the "double player" issue.
                if (client.PlayerObject == null)
                {
                    Debug.Log($"Spawning player object for client {clientId}");
                    var playerSpawn = Instantiate(_playerPrefab);
                    playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                }
                else
                {
                    Debug.Log($"Player object already exists for client {clientId}. Skipping manual spawn.");
                }
            }
        }
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
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
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
            StartCoroutine(CheckWinCondition());
        }
    }

    private IEnumerator CheckWinCondition()
    {
        if (!IsServer) yield break;

        if (_playersAliveIds.Count <= 1)
        {
            Debug.Log("Game Over detected by server. Returning to Lobby.");
            yield return new WaitForSeconds(5f);

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
        _playersAliveIds.Clear();

        if(Round <= MaxRounds)
        {
            Round++;
            NetworkManager.SceneManager.LoadScene("Main", LoadSceneMode.Single);
        }
        else
            NetworkManager.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }
    #endregion
}
