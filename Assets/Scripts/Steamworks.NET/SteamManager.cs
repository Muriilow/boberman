using UnityEngine;
using System;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    public uint appId;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        try
        {
            SteamClient.Init(appId, false);
            Debug.Log("Steamworks Initialized");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void Update()
    {
        SteamClient.RunCallbacks();
    }
    private void OnApplicationQuit()
    {
        try
        {
            SteamClient.Shutdown();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}