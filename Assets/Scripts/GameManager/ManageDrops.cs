using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

public class ManageDrops : NetworkBehaviour
{
    [SerializeField] private Transform wallsParent;
    public static ManageDrops Instance { get; private set; }

    [Header("Tilemaps")]
    public Transform wall;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Tile spawnTile;
    [SerializeField] private Tile[] ground;
    [SerializeField] private Tile[] indestructible;

    [Header("Grid")]
    public Grid<BackgroundTile> walls;
    [SerializeField] private Vector3Int size;
    public Vector3Int origin;

    [Header("Game Variables")]
    [Range(0f, 1f)]
    [SerializeField] private float bricksPercentage;

    [Header("Debug")]
    [SerializeField] private Transform isUsableParent;
    [SerializeField] private Transform hasWallParent;
    private TextMesh[,] hasWall;

    private void Awake()
    {
        
        size = tileMap.size;
        Instance = this;
        walls = new Grid<BackgroundTile>(size.x, size.y);
        hasWall = new TextMesh[size.x, size.y];
        origin = tileMap.origin;
    }

    private void Start()
    {

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
                Vector3Int pos = new Vector3Int(i + origin.x, j + origin.y, origin.z);
                if (CanCreate(i, j, pos, chance))
                {
                    wall = Instantiate(wall, pos, Quaternion.identity);
                    walls.gridArray[i, j].HasWall = true;
                    walls.gridArray[i, j].Wall = wall.gameObject;
                    wall.GetComponent<NetworkObject>().Spawn(true);

                    wall.name = "wall";
                    wall.SetParent(wallsParent);
                }
                hasWall[i, j] = DebugGrid.CreateWorldText(hasWallParent, CanCreate(i, j, pos, chance) ? "1" : "0", pos, 10, Color.white, TextAnchor.MiddleCenter);
            }
    }

    private bool CanCreate(int i, int j, Vector3Int pos, float chance) => walls.gridArray[i, j].IsUsable && ShouldCreate(pos) && chance < bricksPercentage;
    private bool ShouldCreate(Vector3Int _pos)
    {
        Tile _tile = tileMap.GetTile(_pos) as Tile;


        if (_tile == spawnTile) //check if this is spawn TODO!!!
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
                walls.gridArray[i, j] = new BackgroundTile(UsableTile(_pos), false, null, _pos.x, _pos.y);

                DebugGrid.CreateWorldText(isUsableParent, UsableTile(_pos) ? "1" : "0", _pos, 10, Color.white, TextAnchor.MiddleCenter);

                //Debug.Log(i + "," + j);
            }
    }
    
    public void RemoveWalls(int XIndex, int YIndex)
    {
        GameObject wall = walls.gridArray[XIndex, YIndex].Wall;
        Destroy(wall);

        walls.gridArray[XIndex, YIndex].HasWall = false;
        walls.gridArray[XIndex, YIndex].Wall = null;
        hasWall[XIndex, YIndex].text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        
        //Debug.Log(destructible.size);
    }
}
