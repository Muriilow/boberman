using UnityEngine;
using Steamworks;
using System;
public class InviteFriendButton : MonoBehaviour
{
    public SteamId steamId;

    public void Invite()
    {
        try
        {
            SteamLobby.currentLobby.InviteFriend(steamId);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
