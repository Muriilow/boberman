using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class MatchMaker : MonoBehaviour
{
    public TextMeshProUGUI updateText;
    private static UnityTransport _transport;
    public int maxPlayers = 4;
    public string id;
    public string lobbyName = "Lobby";
    public string joinKey = "j";
    [SerializeField] private TMP_InputField code;
    public static string PlayerId { get; private set; }
    
     public void Host()
    {
        CreateLobby();
        updateText.text = "I am host";
    }


    public void Join()
    {
        NetworkManager.Singleton.StartClient();
        updateText.text = "I am client";
    }

    public async void Play()
    {
        updateText.text = "Logging in";
        _transport = Object.FindObjectOfType<UnityTransport>();

        await Login();

        CreateLobby();
    }

    private async void CreateLobby()
    {
        updateText.text = "Creating lobby";

        try
        {
            var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);
            
            var options = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { joinKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                }
            };
            
            var lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
             
            _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

            id = lobby.Id;
            StartCoroutine(HeartBeat(lobby.Id, 15));
            
            NetworkManager.Singleton.StartHost();
            updateText.text = "I am lobby Host";
            Debug.Log(lobby.LobbyCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            updateText.text = "Error creating lobby";
        }
    }

    public static async Task Login()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            var options = new InitializationOptions();
            
            await UnityServices.InitializeAsync(options);
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            
            PlayerId = AuthenticationService.Instance.PlayerId;
        }
    }

    public static IEnumerator HeartBeat(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSeconds(waitTimeSeconds);
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            print("Beat");
            yield return delay; 
        }
    }

    public async void JoinLobbyByCode()
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code.text);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private void OnDestroy()
    {
        try
        {
            StopAllCoroutines();
        }
        catch (Exception e)
        {
            Debug.LogFormat("Failed to destroy");
            throw;
        }
    }
}
