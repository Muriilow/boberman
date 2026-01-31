using System;
using System.Collections;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{

    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button spawnWalls;
    [SerializeField] private ManageDrops ManageDrops;
    private void Awake()
    {
        if(serverButton)
        serverButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        clientButton.onClick.AddListener(() =>
        {
            var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
            transport.targetSteamId = SteamLobby.currentLobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
        });
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        spawnWalls.onClick.AddListener(() =>
        {
            ManageDrops.CreateTiles();
            ManageDrops.CreateWalls();
        });
    }
}
