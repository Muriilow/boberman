using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utilities;
public class PlayerBomb : NetworkBehaviour
{
    [Header("Bomb")]
    [SerializeField] private GameObject _bombPrefab;

    [SerializeField] private NetworkVariable<int> _bombsRemaining;
    [SerializeField] private int _bombAmount;
    [SerializeField] private int _maxBombAmount;

    [Header("Explosion")]
    [SerializeField] private NetworkVariable<int> _explosionRadius;
    [SerializeField] private int _maxRadius;

    [Header("References")]
    [SerializeField] private ManageDrops _manageDrops;

    private void Awake()
    {
        _manageDrops = ManageDrops.Instance;

        _explosionRadius = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        _bombsRemaining = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        _bombAmount = 1;
        _maxBombAmount = 6;
        _maxRadius = 5;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        CanPlaceBombServerRpc(Input.GetKeyDown(KeyCode.Space), position);
    }


    #region Bombs

    [Rpc(SendTo.Server)]
    private void CanPlaceBombServerRpc(bool hasPressedAttack, Vector2 position)
    {
        (int x, int y) = Utilities.Convert.PositionToGrid(position, _manageDrops.origin);

        bool hasBomb = _manageDrops.CheckBombs(x, y);

        if (_bombsRemaining.Value > 0 && hasPressedAttack && !hasBomb)
            SpawnBombServerRpc(position, x, y);
    }

    [Rpc(SendTo.Server)]
    public void SpawnBombServerRpc(Vector2 position, int x, int y)
    {
        GameObject bomb = Instantiate(_bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);
        bomb.GetComponent<BombControl>().Initalize(_explosionRadius.Value, this, position);

        _bombsRemaining.Value--;

        _manageDrops.UpdateGridBomb(true, x, y);
        _manageDrops.UpdateTextBomb(true, x, y);
    }

    #endregion

    #region Upgrades
    [Rpc(SendTo.Server)]
    public void AddBombServerRpc(int amount)
    {
        if (_bombAmount < _maxBombAmount)
        {
            _bombAmount += amount;
            _bombsRemaining.Value += amount;
        }
        else
            _bombsRemaining.Value = _bombAmount;
    }

    [Rpc(SendTo.Server)]
    public void GetBombServerRpc()
    {
        _bombsRemaining.Value++;
    }

    [Rpc(SendTo.Server)]
    public void AddRadiusServerRpc(int amount)
    {
        if (_explosionRadius.Value + amount < _maxRadius)
            _explosionRadius.Value += amount;
        else
            _explosionRadius.Value = _maxRadius;
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch(collision.tag)
        {
            case "BombPick":
            {
                AddBombServerRpc(1);

                collision.GetComponent<BombPickable>().DesappearServerRpc();
                break;
            }
            case "BlastPick":
            {
                AddRadiusServerRpc(1);

                collision.GetComponent<BlastPickable>().DesappearServerRpc();
                break;
            }
            case "SpeedPick":
            {
                GetComponent<PlayerMovement>().AddMoveSpeed(1);

                collision.GetComponent<SpeedPickable>().DesappearServerRpc();
                break;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if(collision.gameObject.tag == "Bomb")
        //{
            //Maybe A placeholder idk
        //}
    }
}
