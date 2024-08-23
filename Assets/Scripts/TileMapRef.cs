using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TilemapData", menuName = "ScriptableObjects/TilemapSO", order = 1)]
public class TileMapRef : ScriptableObject
{
    public Tilemap collision;
    public Tilemap ground;
}
