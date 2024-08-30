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

    public List<Vector3Int> spawnPoints = new List<Vector3Int>();

    private void Awake()
    {
        Instance = this;
        for(int i = 0; i < tileMap.size.x -1; i++) 
        {
            for(int j = 0; j < tileMap.size.y; j++)
            {
                Vector3Int _pos = new Vector3Int(i + tileMap.origin.x, j + tileMap.origin.y, tileMap.origin.z);
                Tile _tile = tileMap.GetTile(_pos) as Tile;

                if(_tile == spawnTile)
                    spawnPoints.Add(_pos);
            }
        }

    }

    public Vector3Int GetSpawnPoint() => spawnPoints[Random.Range(0, spawnPoints.Count)];
}
