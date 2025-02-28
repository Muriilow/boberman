using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.UI.Image;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Tile spawnTile;
    public Stack<Vector3Int> spawnPoints = new Stack<Vector3Int>();

    //Get all the tiles in the tilemap and save the spawnpoints tile into a list
    private void Awake()
    {

        Instance = this;
        for(int i = 0; i < tileMap.size.x -1; i++) 
        {
            for(int j = 0; j < tileMap.size.y; j++)
            {
                Vector3Int _pos = new Vector3Int(i + tileMap.origin.x, j + tileMap.origin.y, tileMap.origin.z);
                Tile _tile = tileMap.GetTile(_pos) as Tile;

                if (_tile == spawnTile)
                {
                    spawnPoints.Push(_pos);
                }
            }
        }

    }

    public override void OnNetworkSpawn()
    {
        //GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }
    public Vector3Int GetSpawnPoint() => spawnPoints.Pop();
}
