using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBomb : NetworkBehaviour
{
    [Header("Bomb")]

    [SerializeField] private GameObject _bombPrefab;
    [SerializeField] private float _bombFuseTime;
    [SerializeField] private NetworkVariable<int> _bombsRemaining = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private int _bombAmount;
    [SerializeField] private int _maxBombAmount;
    [SerializeField] private Queue<GameObject> bombsQueue = new Queue<GameObject>();

    [Header("Explosion")]
    [SerializeField] private Explosion _explosionStartPrefab;
    [SerializeField] private Explosion _explosionMiddlePrefab;
    [SerializeField] private Explosion _explosionEndPrefab;
    [SerializeField] private LayerMask _indestructibleLayer;
    public int _explosionRadius;
    public int _maxRadius;

    [Header("References")]
    [SerializeField] private ManageDrops _manageDrops;
    [SerializeField] private Grid<BackgroundTile> _grid;


    public override void OnNetworkSpawn()
    {
        _maxBombAmount = 6;
        _explosionRadius = 1;
        _maxRadius = 5;
        _bombAmount = 1;
        _bombFuseTime = 3f;
        _manageDrops = ManageDrops.Instance;
        _bombsRemaining.Value = _bombAmount;
        _grid = _manageDrops.walls;
    }


    private void Update()
    {
        if (!IsOwner)
            return;

        if(_bombsRemaining.Value > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(WaitBomb());

        }
            
    }

    private IEnumerator WaitBomb() 
    {

        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        _bombsRemaining.Value--;
        Debug.Log("placing a bomb");

        SpawnBombServerRpc(position);

        yield return new WaitForSeconds(_bombFuseTime);

        StartExplosionServerRpc(position);

        DestroyBombServerRpc();
        _bombsRemaining.Value++;
    }
    [Rpc(SendTo.Server)]
    private void DestroyBombServerRpc()
    {
        bombsQueue.Dequeue().GetComponent<NetworkObject>().Despawn(true);
    }

    [Rpc(SendTo.Server)]
    private void ExplodeServerRpc(Vector2 position, Vector2 direction, int length) //Recursive function to continue exploding in one direction 
    {
        if(length <= 0) //Has the explosion length? 
            return;

        position +=  direction;

        int x = (int)position.x - _manageDrops.origin.x;
        int y = (int)position.y - _manageDrops.origin.y;

        if (CheckForUsableTiles(x, y)) //Check for usable using the array2D information created by the gameManager 
            return;
        else if (CheckForWalls(x, y)) //Check for walls using the array2D information created by the gameManager 
        {
            _manageDrops.RemoveWalls(x, y, position);
            return;
        }

        Explosion explosion = Instantiate(length > 1 ? _explosionMiddlePrefab : _explosionEndPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);
        explosion.SetDirectionClientRpc(direction);

        ExplodeServerRpc(position, direction, length - 1);
    }

    [Rpc(SendTo.Server)]
    private void StartExplosionServerRpc(Vector2 position)
    {

        Explosion explosion = Instantiate(_explosionStartPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(position, Vector2.up, _explosionRadius);
        ExplodeServerRpc(position, Vector2.down, _explosionRadius);
        ExplodeServerRpc(position, Vector2.left, _explosionRadius);
        ExplodeServerRpc(position, Vector2.right, _explosionRadius);
    }
    //Check if the BackgroundTile in this specific location is not usable or if it is usable but has a wall on it
    private bool CheckForUsableTiles(int x, int y) => !_grid.gridArray[x , y].IsUsable;
    private bool CheckForWalls(int x, int y) =>  _grid.gridArray[x, y].HasWall;


    [Rpc(SendTo.Server)]
    public void SpawnBombServerRpc(Vector2 playerPosition)
    {
        GameObject bomb = Instantiate(_bombPrefab, playerPosition, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);
        bombsQueue.Enqueue(bomb);
    }
    public void AddBomb(int amount)
    {
        if (_bombAmount < _maxBombAmount)
        {
            _bombAmount++;
            _bombsRemaining.Value++;
        }
        else
            _bombsRemaining.Value = _bombAmount;
    }

    public void AddRadius(int amount)
    {
        if (_explosionRadius + amount < _maxRadius)
            _explosionRadius += amount;
        else
            _explosionRadius = _maxRadius;
    }
}
