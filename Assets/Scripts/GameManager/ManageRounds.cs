using Netcode.Transports.Facepunch;
using Steamworks;
using UnityEngine;
using Unity.Netcode;
public class ManageRounds : NetworkBehaviour
{
    private static bool _alreadyInitialised = false;
    
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

    private void OnDestroy()
    {
        // Don't reset static flag here to avoid race conditions with multiple objects
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
            return;
        
        ManageDrops.Instance.CreateTiles();
        ManageDrops.Instance.CreateWalls();
    }
}
