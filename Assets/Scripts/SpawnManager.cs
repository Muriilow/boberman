using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public SpawnPoint[] spawnPoints;

    private void Awake()
    {
        Instance = this;
        spawnPoints = GetComponentsInChildren<SpawnPoint>();
    }

    public Transform GetSpawnPoint() => spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
}
