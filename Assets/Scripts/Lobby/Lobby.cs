using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;
using Steamworks.Data;
using UnityEngine.Events;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UIElements;

public class SteamLobby : MonoBehaviour
{
    public static Lobby currentLobby;
    private bool isOwner = false;
    public UnityEvent OnLobbyCreated;
    public UnityEvent OnLobbyJoined;
    public UnityEvent OnLobbyLeave;
    
    public GameObject InLobbyFriend;
    public Transform content;
    
    public Dictionary<SteamId, GameObject> inLobby = new Dictionary<SteamId, GameObject>();
    private void Start()
    {
        DontDestroyOnLoad(this);
        SteamFriends.RequestUserInformation(SteamClient.SteamId);
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnChatMessage += OnChatMessage;
        SteamMatchmaking.OnLobbyMemberDisconnected +=  OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyMemberLeave +=  OnLobbyMemberDisconnected;
        SteamMatchmaking.OnLobbyGameCreated +=  OnLobbyGameCreated;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequest;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
    }

    private void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        Debug.Log($"{friend.Name} invited you to this lobby");
    }
    
    private void OnLobbyGameCreated(Lobby lobby, uint ip, ushort port, SteamId id)
    {
        
    }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} joined the lobby");
        var obj = Instantiate(InLobbyFriend, content);
        obj.GetComponentInChildren<Text>().text = friend.Name;
        //obj.GetComponentInChildren<RawImage>().texture = 
        inLobby.Add(friend.Id, obj);
    }

    private void OnLobbyMemberDisconnected(Lobby lobby, Friend friend)
    {
        Debug.Log($"{friend.Name} disconnected from the lobby");

        if (!inLobby.ContainsKey(friend.Id)) 
            return;
        
        Destroy(inLobby[friend.Id]);
        inLobby.Remove(friend.Id);
    }

    private void OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        Debug.Log($"Incoming chat message from {friend.Name}: {message}");
    }

    private async void OnGameLobbyJoinRequest(Lobby lobby, SteamId id)
    {
        var joinedLobbySuccess = await lobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            Debug.Log($"Failed to join lobby: {joinedLobbySuccess}");
            return;
        }
         
        currentLobby = lobby;
    }

    void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        if (result != Result.OK)
        {
            Debug.Log("Lobby creation failed: " + result);
            return;
        }

        OnLobbyCreated.Invoke();
    }

    private void OnLobbyEntered(Lobby lobby)
    {
        foreach (var user in inLobby.Values)
            Destroy(user);
            
        inLobby.Clear();
        Debug.Log("Lobby entered");
        
        foreach (var member in currentLobby.Members)
        {
            SteamFriends.RequestUserInformation(member.Id);
            var obj = Instantiate(InLobbyFriend, content);
            obj.GetComponentInChildren<TextMeshProUGUI>().text = member.Name;
            
            inLobby.Add(member.Id, obj);
        }
        OnLobbyJoined.Invoke();
    }

    public async void CreateLobbyAsync()
    {
        var result = await CreateLobby();
        if (!result)
            Debug.Log("Could not create lobby");
        
    }

    private async Task<bool> CreateLobby()
    {
        try
        {
            var createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(4);
            if (!createLobbyOutput.HasValue)
            {
                Debug.Log("Lobby creation failed.");
                return false;
            }
            currentLobby = createLobbyOutput.Value;
            currentLobby.SetFriendsOnly();
            currentLobby.SetJoinable(true);
            isOwner = true;
            
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("Failed to create multiplayer lobby: " + e);
            return false;
        }
    }

    public void LeaveLobby()
    {
        try
        {
            currentLobby.Leave();
            OnLobbyLeave.Invoke();
            foreach (var user in inLobby.Values)
                Destroy(user);
            
            inLobby.Clear();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
