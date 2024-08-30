using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<T>
{
    private int width;
    private int height;
    public T[,] gridArray;

    public Grid(int width, int height)
    {
        this.width = width;
        this.height = height;

        gridArray = new T[width, height];

    }
}

public class BackgroundTile
{
    public bool IsUsable { get; set; }
    public GameObject wall;

    public BackgroundTile(bool isUsable, GameObject wall)
    {
        IsUsable = isUsable;
        this.wall = wall;
    }
}

