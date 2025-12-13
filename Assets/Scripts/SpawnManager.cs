using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using static UnityEngine.UI.Image;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
    [SerializeField] private Tilemap _tileMap;
    [SerializeField] private Tile _spawnTile;
    private Stack<Vector3Int> _spawnPoints = new Stack<Vector3Int>();
    
    private void Awake()
    {
        Instance = this;
        
        for(var i = 0; i < _tileMap.size.x -1; i++) 
        {
            for(var j = 0; j < _tileMap.size.y; j++)
            {
                var pos = new Vector3Int(i + _tileMap.origin.x, j + _tileMap.origin.y, _tileMap.origin.z);
                var tile = _tileMap.GetTile(pos) as Tile;

                if (tile == _spawnTile)
                    _spawnPoints.Push(pos);
            }
        }

    }

    public Vector3Int GetSpawnPoint() => _spawnPoints.Pop();
}
