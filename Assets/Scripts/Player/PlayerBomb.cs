using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerBomb : NetworkBehaviour
{
    [Header("Bomb")]
    public GameObject bombPrefab;
    public float bombFuseTime = 3f;
    public int bombAmount;
    public int bombsRemaining;

    [Header("Explosion")]
    [SerializeField] private Explosion explosionStartPrefab;
    [SerializeField] private Explosion explosionMiddlePrefab;
    [SerializeField] private Explosion explosionEndPrefab;
    [SerializeField] private LayerMask indestructibleLayer;
    public int explosionRadius;

    [SerializeField] private ManageDrops manageDrops;
    [SerializeField] private Grid<BackgroundTile> grid;
    //The list of bombs going to be destroyed
    //private Queue<GameObject> bombs = new Queue<GameObject>();
    private void Awake()
    {
        manageDrops = ManageDrops.Instance;
        bombsRemaining = bombAmount;
        grid = manageDrops.walls;
    }


    private void Update()
    {
        if (!IsOwner)
            return;

        if(bombsRemaining > 0 && Input.GetKeyDown(KeyCode.Space))
            PlaceBombServerRpc();
    }

    private IEnumerator WaitBomb() 
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        bombsRemaining--;

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.GetComponent<NetworkObject>().Spawn(true);

        yield return new WaitForSeconds(bombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        Explosion explosion = Instantiate(explosionStartPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);

        ExplodeServerRpc(position, Vector2.up, explosionRadius);
        ExplodeServerRpc(position, Vector2.down, explosionRadius);
        ExplodeServerRpc(position, Vector2.left, explosionRadius);
        ExplodeServerRpc(position, Vector2.right, explosionRadius);

        Destroy(bomb);
        bombsRemaining++;
    }

    [Rpc(SendTo.Server)]
    private void ExplodeServerRpc(Vector2 position, Vector2 direction, int length) //Recursive function to continue exploding in one direction 
    {
        if(length <= 0) //Has the explosion length? 
            return;

        position +=  direction;

        int x = (int)position.x - manageDrops.origin.x;
        int y = (int)position.y - manageDrops.origin.y;

        if (CheckForUsableTiles(x, y)) //Check for usable using the array2D information created by the gameManager 
            return;
        else if (CheckForWalls(x, y)) //Check for walls using the array2D information created by the gameManager 
        {
            manageDrops.RemoveWalls(x, y);
            return;
        }

        Explosion explosion = Instantiate(length > 1 ? explosionMiddlePrefab : explosionEndPrefab, position, Quaternion.identity);
        explosion.GetComponent<NetworkObject>().Spawn(true);
        explosion.SetDirectionClientRpc(direction);

        ExplodeServerRpc(position, direction, length - 1);
    }

    //Check if the BackgroundTile in this specific location is not usable or if it is usable but has a wall on it
    private bool CheckForUsableTiles(int x, int y) => !grid.gridArray[x , y].IsUsable;
    private bool CheckForWalls(int x, int y) =>  grid.gridArray[x, y].HasWall;

    [Rpc(SendTo.Server)]
    private void PlaceBombServerRpc()
    {
        StartCoroutine(WaitBomb());
    }
}
