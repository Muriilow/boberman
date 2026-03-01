using Netcode.Transports.Facepunch;
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

        if (SteamLobby.Instance != null)
        {
            if (SteamLobby.Instance.isOwner)
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
                }
                _alreadyInitialised = true;
                NetworkManager.Singleton.StartClient();
                Debug.Log("Starting client automatically (Steam Member)");
            }
        }
        else
        {
            Debug.LogWarning("SteamLobby instance is null. Defaulting to StartHost for local testing.");
            _alreadyInitialised = true;
            NetworkManager.Singleton.StartHost();
        }
    }

    private void OnDestroy()
    {
        // Don't reset _alreadyInitialised here to avoid race conditions with multiple starts.
        // It will reset on app restart or manual reset if needed.
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
            return;
        
        ManageDrops.Instance.CreateTiles();
        ManageDrops.Instance.CreateWalls();
    }
}
