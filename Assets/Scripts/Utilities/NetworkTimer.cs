using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTimer
{
    private float timer;
    public float MinTimeBetweenTicks {  get; }
    public int CurrentTick { get; private set; }

    public NetworkTimer(float serverTickRate)
    {
        MinTimeBetweenTicks = 1f / serverTickRate;
    }

    public void Update(float deltaTime)
    {
        timer += deltaTime;
    }

    public bool ShouldTick()
    {
        if (timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            CurrentTick++;
            return true;
        }

        return false;
    }
}

public class CircularBuffer<T>
{
    T[] buffer;
    private int bufferSize;

    public CircularBuffer(int bufferSize)
    {
        this.bufferSize = bufferSize;
        buffer = new T[bufferSize];
    }
    //Add the element to that index 
    public void Add(T item, int index) => buffer[index % bufferSize] = item;

    //Get the element of that index
    public T Get(int index) => buffer[index % bufferSize];

    //clears the buffer array craeting a new empty array
    private void Clear() => buffer = new T[bufferSize];
}
