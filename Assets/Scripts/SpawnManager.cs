using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
    [SerializeField] private Tilemap _tileMap;
    [SerializeField] private Tile _spawnTile;
    private Stack<Vector3Int> _spawnPoints = new Stack<Vector3Int>();
    private NetworkVariable<int> _availableSpawns = new NetworkVariable<int>();
    private NetworkVariable<bool> _isInitialized = new NetworkVariable<bool>(false);
    
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;
        
        InitializeSpawnPoints();
        _isInitialized.Value = true;

        base.OnNetworkSpawn();
    }

    private void InitializeSpawnPoints()
    {
        _spawnPoints.Clear();

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
        _availableSpawns.Value = _spawnPoints.Count;
    }

    public Vector3Int GetSpawnPoint()
    {
        if (!_isInitialized.Value && IsServer)
        {
            Debug.Log("Not initialized yet: WARNING 01");
            InitializeSpawnPoints();
            _isInitialized.Value = true;
        }
        _availableSpawns.Value = _spawnPoints.Count;
        var spawnPoint = _tileMap.CellToWorld(_spawnPoints.Pop());
        return _spawnPoints.Pop();
    }
    public bool CanSpawn() => IsServer && _isInitialized.Value &&  _availableSpawns.Value > 0;
}
