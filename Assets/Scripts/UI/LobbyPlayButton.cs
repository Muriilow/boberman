using UnityEngine;
using Steamworks;
public class LobbyPlayButton : MonoBehaviour
{
    [SerializeField] private GameObject playButton;
    private void Start()
    {
        if (SteamLobby.currentLobby.Owner.Id == SteamClient.SteamId)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
    }

    public void OnClickStartGame()
    {
        if (SteamLobby.currentLobby.Owner.Id != SteamClient.SteamId)
            return;

        SteamLobby.currentLobby.SetData("startGame", "1");
    }
}
