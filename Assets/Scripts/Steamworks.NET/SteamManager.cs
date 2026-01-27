using UnityEngine;
using System;
public class SteamManager : MonoBehaviour
{
    public uint appId;

    private void Awake()
    {
        DontDestroyOnLoad(this);

        try
        {
            Steamworks.SteamClient.Init(appId, true);
            Debug.Log("Steamworks Initialized");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            Steamworks.SteamClient.Shutdown();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}