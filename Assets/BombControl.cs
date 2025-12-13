using Unity.Netcode;
using UnityEngine;

public class BombControl : NetworkBehaviour
{
    [SerializeField] private Explosion _explosionStartPrefab;
    [SerializeField] private Explosion _explosionMiddlePrefab;
    [SerializeField] private Explosion _explosionEndPrefab;

    [SerializeField] private ManageDrops _manageDrops;
    [SerializeField] private PlayerBomb _playerBomb;

    [SerializeField] private int _explosionRadius;
    [SerializeField] private float _bombFuseTime = 3f;
    [SerializeField] private Vector2 _initialPos;
    private void Awake()
    {
        _manageDrops = ManageDrops.Instance;
    }

    public void Initialize(int explosionRadius, PlayerBomb playerBomb, Vector2 position)
    {
        _explosionRadius = explosionRadius;
        _playerBomb = playerBomb;
        _initialPos = position;
    }

    private void Update()
    {
        if (_bombFuseTime < 0)
        {
            StartExplosionServerRpc();
            DestroyBombServerRpc();
        }
        else
            _bombFuseTime -= Time.deltaTime;
    }
    #region Explosions

    [Rpc(SendTo.Server)]
    private void StartExplosionServerRpc()
    {
        var explosion = Instantiate(_explosionStartPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(transform.position, Vector2.up, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.down, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.left, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.right, _explosionRadius);
    }

    [Rpc(SendTo.Server)]
    private void ExplodeServerRpc(Vector2 position, Vector2 direction, int length)
    {
        if (length < 1)
            return;

        position += direction;

        var (x, y) = Utilities.Convert.PositionToGrid(position, _manageDrops.origin);

        if (_manageDrops.CheckForUsableTiles(x, y))
            return;

        if (_manageDrops.CheckForWalls(x, y)) 
        {
            _manageDrops.RemoveWalls(position);
            return;
        }

        var isLast = length > 1 ? _explosionMiddlePrefab : _explosionEndPrefab; 
        var explosion = Instantiate(isLast, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);
        explosion.SetDirectionClientRpc(direction);

        ExplodeServerRpc(position, direction, length - 1);
    }

    #endregion

    [Rpc(SendTo.Server)]
    private void DestroyBombServerRpc()
    {
        var (x, y) = Utilities.Convert.PositionToGrid(transform.position, _manageDrops.origin);

        _manageDrops.UpdateGridBomb(false, x, y);
        _manageDrops.UpdateTextBomb(false, x, y);

        _playerBomb.GetBombServerRpc();
        GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
            GetComponent<CircleCollider2D>().isTrigger = false;
    }
}
