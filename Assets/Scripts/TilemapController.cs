using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapController : MonoBehaviour
{
    [SerializeField] private TileMapRef scriptableObj;
    [SerializeField] private tilemapType tilemap;
    [SerializeField] private Tilemap type;
    void Awake()
    {
        type = GetComponent<Tilemap>();

        switch (tilemap)
        {
            case tilemapType.ground:
                {
                    scriptableObj.ground = type;
                    break;
                }
            case tilemapType.collision:
                {
                    scriptableObj.collision = type;
                    break;
                }
        }
    }
    private void Start()
    {

    }

}
public enum tilemapType
{
    ground,
    collision
}