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
    [SerializeField] private NetworkVariable<int> _bombAmount;
    [SerializeField] private int _maxBombAmount;
    [SerializeField] private Queue<GameObject> bombsQueue = new Queue<GameObject>();

    [Header("Explosion")]
    [SerializeField] private Explosion _explosionStartPrefab;
    [SerializeField] private Explosion _explosionMiddlePrefab;
    [SerializeField] private Explosion _explosionEndPrefab;
    [SerializeField] private LayerMask _indestructibleLayer;
    [SerializeField] private NetworkVariable<int> _explosionRadius;
    [SerializeField] private int _maxRadius;

    [Header("References")]
    [SerializeField] private ManageDrops _manageDrops;

    private void Awake()
    {
        _bombsRemaining = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        _bombAmount = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        _explosionRadius = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        _maxBombAmount = 6;
        _maxRadius = 5;
        _bombFuseTime = 3f;

        _manageDrops = ManageDrops.Instance;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsClient) 
            return;

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
    private bool HasBombInPosition(int x, int y)
    {
        if (_manageDrops._bombs.gridArray[x, y].hasBomb)
            return true;

        return false;
    }

    [Rpc(SendTo.Server)]
    private void CanPlaceBombServerRpc(int bombsRemaining, bool hasPressedAttack, Vector2 position)
    {
        (int x, int y) = ConvertPositionToGrid(position);
        bool hasBomb = HasBombInPosition(x, y);

        if(bombsRemaining > 0 && hasPressedAttack && !hasBomb)
            StartCoroutine(WaitBomb(position));
    }

    private IEnumerator WaitBomb(Vector2 position) 
    {
        ReduceBombs(true); 

        SpawnBombServerRpc(position);

        yield return new WaitForSeconds(_bombFuseTime);

        StartExplosionServerRpc(position);

        DestroyBombServerRpc(position);

        ReduceBombs(false);
    }

    private void ReduceBombs(bool reduce)
    {
        if(reduce)
            _bombsRemaining.Value--;
        else
            _bombsRemaining.Value++;
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
    private void StartExplosionServerRpc(Vector2 position)
    {

        Explosion explosion = Instantiate(_explosionStartPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(position, Vector2.up, _explosionRadius.Value);
        ExplodeServerRpc(position, Vector2.down, _explosionRadius.Value);
        ExplodeServerRpc(position, Vector2.left, _explosionRadius.Value);
        ExplodeServerRpc(position, Vector2.right, _explosionRadius.Value);
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
        if (_bombAmount.Value < _maxBombAmount)
        {
            _bombAmount.Value += amount;
            _bombsRemaining.Value += amount;
        }
        else
            _bombsRemaining.Value = _bombAmount.Value;
    }

    public void AddRadius(int amount)
    {
        if (_explosionRadius.Value + amount < _maxRadius)
            _explosionRadius.Value += amount;
        else
            _explosionRadius.Value = _maxRadius;
    }
    #endregion

    private (int, int) ConvertPositionToGrid(Vector2 position)
    {
        int x = (int)position.x - _manageDrops.origin.x;
        int y = (int)position.y - _manageDrops.origin.y;

        return (x, y);
    }
}
