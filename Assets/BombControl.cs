using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float _bombFuseTime;
    [SerializeField] private Vector2 _initialPos;
    private void Awake()
    {
        _bombFuseTime = 3f;
        _manageDrops = ManageDrops.Instance;
    }

    public void Initalize(int explosionRadius, PlayerBomb playerBomb, Vector2 position)
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
        Explosion explosion = Instantiate(_explosionStartPrefab, transform.position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(transform.position, Vector2.up, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.down, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.left, _explosionRadius);
        ExplodeServerRpc(transform.position, Vector2.right, _explosionRadius);
    }

    [Rpc(SendTo.Server)]
    private void ExplodeServerRpc(Vector2 position, Vector2 direction, int length) //Recursive function to continue exploding in one direction 
    {
        if (length <= 0) //Has the explosion length? 
            return;

        position += direction;

        (var x, var y) = Utilities.Convert.PositionToGrid(position, _manageDrops.origin);

        //Instead of checking for collisions we check the multidimensional array created by the wallManager
        if (_manageDrops.CheckForUsableTiles(x, y)) //Did the explosion run into a unbreakable wall?
            return;

        if (_manageDrops.CheckForWalls(x, y)) //Did the explosion run into a normal wall? 
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

    [Rpc(SendTo.Server)]
    private void DestroyBombServerRpc()
    {
        (var x, var y) = Utilities.Convert.PositionToGrid(transform.position, _manageDrops.origin);

        _manageDrops.UpdateGridBomb(false, x, y);
        _manageDrops.UpdateTextBomb(false, x, y);

        _playerBomb.GetBombServerRpc();
        GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            GetComponent<CircleCollider2D>().isTrigger = false;
        }
    }
}
