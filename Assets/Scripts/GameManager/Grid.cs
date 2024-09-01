using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    public int Width { get; set; }
    public int Height { get; set; }

    public T[,] gridArray;

    public Grid(int width, int height)
    {
        this.Width = width;
        this.Height = height;

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

    public BackgroundTile(bool isUsable, bool hasWall, GameObject wall, int x, int y)
    {
        IsUsable = isUsable;
        HasWall = hasWall;
        Wall = wall;
        X = x;
        Y = Y;
    }
}

