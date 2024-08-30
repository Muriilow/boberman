using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;


public class ManageDrops : NetworkBehaviour
{
    [SerializeField] private Transform wallsParent;

    #region Tilemaps
    public Transform wall;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Tile spawnTile;
    [SerializeField] private Tile[] ground;
    [SerializeField] private Tile[] indestructible;
    #endregion

    private Grid<BackgroundTile> walls;

    [SerializeField] private Vector3Int size;
    [SerializeField] private Vector3Int origin;

    [Range(0f, 1f)]
    [SerializeField] private float bricksPercentage; 
    private void Awake()
    {
        size = tileMap.size;

        walls = new Grid<BackgroundTile>(size.x, size.y);
    }

    private void Start()
    {
        origin = tileMap.origin;
        //Debug.Log(origin);

        //CreateTilesRpc();
        //CreateWallsRpc();
    }
    [Rpc(SendTo.Server)]
    public void CreateWallsServerRpc()
    {

        for (int i = 0; i < size.x - 1; i++) // really dont know why I need to subtract 1
            for (int j = 0; j < size.y; j++)
            {
                float chance = UnityEngine.Random.Range(0f, 1f);
                Vector3Int _pos = new Vector3Int(i + origin.x, j + origin.y, origin.z);
                if (walls.gridArray[i, j].IsUsable && ShouldCreate(_pos) && chance < bricksPercentage)
                {
                    wall = Instantiate(wall, _pos, Quaternion.identity);
                    walls.gridArray[i, j].wall = wall.gameObject;
                    wall.GetComponent<NetworkObject>().Spawn(true);

                    wall.name = "wall";
                    wall.SetParent(wallsParent);
                }

            }
    }

    private bool ShouldCreate(Vector3Int _pos)
    {
        Tile _tile = tileMap.GetTile(_pos) as Tile;


        if (_tile == spawnTile) //check if this is spawn
            return false;

        for (int i = 0; i < ground.Length; i++)
            if (_tile == ground[i]) //Check if this is blocks
                return true;
        
        return false;
    }


    private bool UsableTile(Vector3Int _pos)
    {
        Tile _tile = tileMap.GetTile(_pos) as Tile;

        for (int i = 0; i < indestructible.Length; i++)
            if (_tile == indestructible[i])
                return false;

        return true;
    }

    [Rpc(SendTo.Server)]
    public void CreateTilesServerRpc()
    {

        //Create the tiles for all the grid, and set the isUsable variable to be true or false
        for (int i = 0; i < size.x - 1; i++)
            for (int j = 0; j < size.y; j++)
            {
                Vector3Int _pos = new Vector3Int(i + origin.x, j + origin.y, origin.z);
                walls.gridArray[i, j] = new BackgroundTile(UsableTile(_pos), null);

                //Debug.Log(i + "," + j);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log(destructible.size);
    }
}
