using Steamworks;
using TMPro;
using UnityEngine;

public class FriendsList : MonoBehaviour
{
    [SerializeField] private Transform friendsList;
    [SerializeField] private GameObject friendPrefab;
    
    void Start()
    {
        if (!SteamClient.IsValid)
            return;

        InitFriends();
    }

    private void InitFriends()
    {
        foreach (var friend in SteamFriends.GetFriends())
        {
            SteamFriends.RequestUserInformation(friend.Id);
            Debug.Log(friend.Name);
            var newFriend = Instantiate(friendPrefab, friendsList);
            newFriend.GetComponentInChildren<TextMeshProUGUI>().text = friend.Name;
            newFriend.GetComponentInChildren<InviteFriendButton>().steamId = friend.Id;
        }
    }
}
