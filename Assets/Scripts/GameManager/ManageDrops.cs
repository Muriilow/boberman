using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using Utilities;

public class ManageDrops : NetworkBehaviour
{
    [SerializeField] private Transform _wallsParent;
     public static ManageDrops Instance {get; private set;}
     
    [Header("Tilemaps")]
    [SerializeField] private Transform _wall;
    [SerializeField] private Tilemap _tileMap;
    [SerializeField] private Tile _spawnTile;
    [SerializeField] private Tile _spawnBoundariesTile;
    [SerializeField] private Tile[] _ground;
    [SerializeField] private Tile[] _indestructible;

    [Header("Grid")]
    private Grid<BackgroundTile> _walls;
    private NetworkVariable<GridStruct> _bombs; 
    [SerializeField] private Vector3Int _size;
    public Vector3Int origin;

    [Header("Game Variables")]
    [Range(0f, 1f)]
    [SerializeField] private float _bricksPercentage;
    [Range(0f, 1f)]
    [SerializeField] private float _powerUpPercentage;

    [Header("Debug")]
    [SerializeField] private Transform _isUsableParent;
    [SerializeField] private Transform _hasWallParent;
    [SerializeField] private Transform _hasBombParent;
    private TextMesh[,] _hasWall;
    private TextMesh[,] _hasBomb;

    [Header("Itens")]
    [SerializeField] private Speed _speedPw;
    [SerializeField] private Blast _blastPw;
    [SerializeField] private Bomb _bombPw;
    private void Awake()
    {
        Instance = this;

        _size = _tileMap.size;

        //Maybe revert this to a normal grid class?
        _bombs = new NetworkVariable<GridStruct>(new GridStruct(_size.x, _size.y),
                                                NetworkVariableReadPermission.Everyone,
                                                NetworkVariableWritePermission.Server); 

        _walls = new Grid<BackgroundTile>(_size.x, _size.y);

        _hasWall = new TextMesh[_size.x, _size.y];
        _hasBomb = new TextMesh[_size.x, _size.y];

        origin = _tileMap.origin;
    }


    #region Walls
    public void CreateWalls()
    {
        if (!IsServer)
            return;

        for (int i = 0; i < _size.x - 1; i++) // really don't know why I need to subtract 1
            for (int j = 0; j < _size.y; j++)
            {
                float wallChances = UnityEngine.Random.Range(0f, 1f);
                float powerUpChances = UnityEngine.Random.Range(0f, 1f);

                Vector3Int pos = new Vector3Int(i + origin.x, j + origin.y, origin.z);

                if (CanCreateWall(i, j, pos, wallChances))
                {
                    _wall = Instantiate(_wall, pos, Quaternion.identity);
                    UpdateGridWall(true, _wall.gameObject, i, j);
                    _walls.gridArray[i, j].Item = GetPowerUp(powerUpChances);
                    _wall.GetComponent<NetworkObject>().Spawn(true);

                    _wall.name = "wall";
                    _wall.SetParent(_wallsParent);
                }

                _hasWall[i, j] = DebugGrid.CreateWorldText(_hasWallParent,
                                                            CanCreateWall(i, j, pos, wallChances) ? "1" : "0",
                                                            pos,
                                                            10,
                                                            Color.white,
                                                            TextAnchor.MiddleCenter);
                
                _hasBomb[i, j] = DebugGrid.CreateWorldText(_hasBombParent,
                                                        "0",
                                                        pos,
                                                        10,
                                                        Color.white,
                                                        TextAnchor.MiddleCenter);
            }
    }

    public void RemoveWalls(Vector2 pos)
    {
        if (!IsServer)
            return;

        int x = (int)pos.x - origin.x;
        int y = (int)pos.y - origin.y;

        GameObject wall = _walls.gridArray[x, y].Wall;
        GameObject item = _walls.gridArray[x, y].Item;

        //Activating the animation before the wall destroy
        wall.GetComponent<Animator>().SetBool("Destroy", true);

        UpdateGridWall(false, null, x, y);

        UpdateTextWall(false, x, y);

        if (item == null)
            return;

        GameObject powerUp = Instantiate(item, pos, Quaternion.identity); //Creating the superpowers
        powerUp.GetComponent<NetworkObject>().Spawn(true);
    }

    private GameObject GetPowerUp(float chance)
    {
        if (chance >= _powerUpPercentage)
            return null;

        int index = UnityEngine.Random.Range(0, 3);
        switch (index)
        {
            case 0:
                return _speedPw.prefab;
            case 1:
                return _blastPw.prefab;
            case 2:
                return _bombPw.prefab;

            default:
                return null;
        }
    }
    #endregion

    #region Tiles

    public void CreateTiles()
    {
        if (!IsServer)
            return;

        //Create the tiles for all the grid, and set the isUsable variable to be true or false
        for (int i = 0; i < _size.x - 1; i++)
            for (int j = 0; j < _size.y; j++)
            {
                Vector3Int pos = new Vector3Int(i + origin.x, j + origin.y, origin.z);
                _walls.gridArray[i, j] = new BackgroundTile(UsableTile(pos), false, null, null, pos.x, pos.y);
                _bombs.Value.gridArray[i, j] = new BackgroundBomb(UsableTile(pos), false, pos.x, pos.y);

                DebugGrid.CreateWorldText(_isUsableParent,
                                            UsableTile(pos) ? "1" : "0",
                                            pos,
                                            10,
                                            Color.white,
                                            TextAnchor.MiddleCenter);
            }
    }

    private bool UsableTile(Vector3Int pos) //Should this tile be usable?
    {
        Tile _tile = _tileMap.GetTile(pos) as Tile;

        for (int i = 0; i < _indestructible.Length; i++)
            if (_tile == _indestructible[i])
                return false;

        return true;
    }

    private bool CheckTile(Vector3Int pos)
    {
        Tile tile = _tileMap.GetTile(pos) as Tile;


        if (tile == _spawnTile || tile == _spawnBoundariesTile) //check if this is spawn TODO!!!
            return false;

        for (int i = 0; i < _ground.Length; i++)
            if (tile == _ground[i]) //Check if this is ground
                return true;

        return false;
    }
    #endregion

    #region UpdateDebug
    public void UpdateTextBomb(bool hasPlace, int x, int y)
    {
        if (hasPlace)
            _hasBomb[x, y].text = "1";
        else
            _hasBomb[x, y].text = "0";
    }
    public void UpdateTextWall(bool hasPlace, int x, int y)
    {
        if (hasPlace)
            _hasWall[x, y].text = "1";
        else
            _hasWall[x, y].text = "0";
    }
    #endregion

    #region UpdateGrid
    public void UpdateGridBomb(bool hasBomb, int x, int y)
    {
        _bombs.Value.gridArray[x, y].hasBomb = hasBomb;
    }
    public void UpdateGridWall(bool hasWall, GameObject wall, int x, int y)
    {
        _walls.gridArray[x, y].HasWall = hasWall;
        _walls.gridArray[x, y].Wall = wall;
    }
    #endregion

    #region BooleanChecks
    public bool CheckBombs(int x, int y) => _bombs.Value.gridArray[x, y].hasBomb;
    public bool CheckForUsableTiles(int x, int y) => !_walls.gridArray[x, y].IsUsable;
    public bool CheckForWalls(int x, int y) => _walls.gridArray[x, y].HasWall;
    private bool CanCreateWall(int i, int j, Vector3Int pos, float chance) => _walls.gridArray[i, j].IsUsable
                                                                              && CheckTile(pos)
                                                                              && chance < _bricksPercentage;
    #endregion
}
