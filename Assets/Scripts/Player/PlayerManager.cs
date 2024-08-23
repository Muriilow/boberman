using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SpawnPlayerRpc();
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    [Rpc(SendTo.Server)]
    private void SpawnPlayerRpc()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        transform.position = spawnPoint.position;
    }
}
