using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Grid<T>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public T[,] gridArray;

    public Grid(int width, int height)
    {
        Width = width;
        Height = height;

        gridArray = new T[width, height];

    }
}

public class BackgroundTile
{
    public bool IsUsable { get; set; }
    public bool HasWall {  get; set; }
    public int X { get; set; }
    public int Y { get; set; }

    public GameObject Wall { get; set; }
    public GameObject Item { get; set; }
    public BackgroundTile(bool isUsable, bool hasWall, GameObject wall, GameObject item, int x, int y)
    {
        IsUsable = isUsable;
        HasWall = hasWall;
        Wall = wall;
        X = x;
        Y = y;
        Item = item;
    }
}

public struct BackgroundBomb : INetworkSerializable, System.IEquatable<BackgroundBomb>
{
    public bool isUsable;
    public bool hasBomb;
    public int X;
    public int Y;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref isUsable);
        serializer.SerializeValue(ref hasBomb);
        serializer.SerializeValue(ref X);
        serializer.SerializeValue(ref Y);
    }

    public bool Equals(BackgroundBomb other)
    {
        if (other.X == X && other.Y == Y && other.isUsable == isUsable && other.hasBomb)
            return true;

        return false;
    }

    public BackgroundBomb(bool isUsable, bool hasBomb, int x, int y)
    {
        this.isUsable = isUsable;
        this.hasBomb = hasBomb;
        X = x;
        Y = y;
    }
}

public struct GridStruct : INetworkSerializable
{
    private int _width;
    private int _height;

    public BackgroundBomb[,] gridArray;

    public GridStruct(int width, int height)
    {
        _width = width; 
        _height = height;
        gridArray = new BackgroundBomb[width, height];
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _width);
        serializer.SerializeValue(ref _height);

        if (serializer.IsReader)
        {
            gridArray = new BackgroundBomb[_width, _height];
        }
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            { 
                // Serialize each item in the array
                serializer.SerializeValue(ref gridArray[i, j]);
            }
        }
    }
}


