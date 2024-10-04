using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerBomb : NetworkBehaviour
{
    [Header("Bomb")]
    [SerializeField] public bool canPlaceBomb = false;
    [SerializeField] private GameObject _bombPrefab;
    [SerializeField] private float _bombFuseTime;
    [SerializeField] private NetworkVariable<int> _bombsRemaining;
    [SerializeField] private int _bombAmount;
    [SerializeField] private int _maxBombAmount;
    [SerializeField] private Queue<GameObject> bombsQueue = new Queue<GameObject>();

    [Header("Explosion")]
    [SerializeField] private Explosion _explosionStartPrefab;
    [SerializeField] private Explosion _explosionMiddlePrefab;
    [SerializeField] private Explosion _explosionEndPrefab;
    [SerializeField] private LayerMask _indestructibleLayer;
    [SerializeField] private int _explosionRadius;
    [SerializeField] private int _maxRadius;

    [Header("References")]
    [SerializeField] private ManageDrops _manageDrops;

    private void Awake()
    {
        _manageDrops = ManageDrops.Instance;

        _bombsRemaining = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        _bombAmount = 1;
        _explosionRadius = 1;
        _maxBombAmount = 6;
        _maxRadius = 5;
        _bombFuseTime = 3f;
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        CanPlaceBombServerRpc(_bombsRemaining.Value, Input.GetKeyDown(KeyCode.Space), position);
    }


    #region Bombs

    [Rpc(SendTo.Server)]
    private void CanPlaceBombServerRpc(int bombsRemaining, bool hasPressedAttack, Vector2 position)
    {
        (int x, int y) = ConvertPositionToGrid(position);

        bool hasBomb = _manageDrops.CheckBombs(x, y);

        if(bombsRemaining > 0 && hasPressedAttack && !hasBomb)
            StartCoroutine(WaitBomb(position, RpcTarget.Single(OwnerClientId, RpcTargetUse.Temp)));
    }

    private IEnumerator WaitBomb(Vector2 position, RpcParams rpcParams) 
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        var clientObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

        PlayerBomb clientScript = clientObj.GetComponent<PlayerBomb>();

        Debug.Log(clientObj);
        Debug.Log(clientScript);
        Debug.Log(clientId);

        clientScript._bombsRemaining.Value--;

        SpawnBombServerRpc(position);

        yield return new WaitForSeconds(_bombFuseTime);

        StartExplosionServerRpc(position, _explosionRadius);

        DestroyBombServerRpc(position);

        clientScript._bombsRemaining.Value++;
    }


    [Rpc(SendTo.Server)]
    public void SpawnBombServerRpc(Vector2 position)
    {
        GameObject bomb = Instantiate(_bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);

        (int x, int y) = ConvertPositionToGrid(position);

        _manageDrops.UpdateGridBomb(true, bomb, x, y);
        _manageDrops.UpdateTextBomb(true, x, y);

        bombsQueue.Enqueue(bomb);
    }

    [Rpc(SendTo.Server)]
    private void DestroyBombServerRpc(Vector2 position)
    {
        bombsQueue.Dequeue().GetComponent<NetworkObject>().Despawn(true);

        (var x, var y) = ConvertPositionToGrid(position);
        _manageDrops.UpdateGridBomb(false, null, x, y);
        _manageDrops.UpdateTextBomb(false, x, y);
    }
    #endregion

    #region Explosions
    [Rpc(SendTo.Server)]
    private void StartExplosionServerRpc(Vector2 position, int explosionRadius)
    {

        Explosion explosion = Instantiate(_explosionStartPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(position, Vector2.up, explosionRadius);
        ExplodeServerRpc(position, Vector2.down, explosionRadius);
        ExplodeServerRpc(position, Vector2.left, explosionRadius);
        ExplodeServerRpc(position, Vector2.right, explosionRadius);
    }

    [Rpc(SendTo.Server)]
    private void ExplodeServerRpc(Vector2 position, Vector2 direction, int length) //Recursive function to continue exploding in one direction 
    {
        if(length <= 0) //Has the explosion length? 
            return;

        position +=  direction;

        (int x, int y) = ConvertPositionToGrid(position);


        if (_manageDrops.CheckForUsableTiles(x, y)) //Check for usable using the array2D information created by the gameManager 
            return;
        else if (_manageDrops.CheckForWalls(x, y)) //Check for walls using the array2D information created by the gameManager 
        {
            _manageDrops.RemoveWalls(position);
            return;
        }

        Explosion explosion = Instantiate(length > 1 ? _explosionMiddlePrefab : _explosionEndPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);
        explosion.SetDirectionClientRpc(direction);

        ExplodeServerRpc(position, direction, length - 1);
    }
    #endregion

    #region Upgrades
    public void AddBomb(int amount)
    {
        if (!IsOwner)
            return;

        if (_bombAmount < _maxBombAmount)
        {
            _bombAmount += amount;
            _bombsRemaining.Value += amount;
        }
        else
            _bombsRemaining.Value = _bombAmount;
    }

    public void AddRadius(int amount)
    {
        if (!IsOwner)
            return;

        if (_explosionRadius + amount < _maxRadius)
            _explosionRadius += amount;
        else
            _explosionRadius = _maxRadius;
    }
    #endregion

    private (int, int) ConvertPositionToGrid(Vector2 position)
    {
        int x = (int)position.x - _manageDrops.origin.x;
        int y = (int)position.y - _manageDrops.origin.y;

        return (x, y);
    }
}
