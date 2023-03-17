using QFSW.QC;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartbeatTimer;
    private bool isActive;

    public async void OnOpenMenu()
    {
        isActive = true;
        Debug.Log("OnOpenMenu");
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

    }

    public void OnCloseMenu()
    {
        isActive = false;

    }

    private void Update()
    {
        if (isActive) HandleLobbyHeartBeat();
    }

    private async void HandleLobbyHeartBeat()
    {
        float heartbeatTimerMax = 15;

        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                heartbeatTimer = heartbeatTimerMax;
                Debug.Log("sending heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    [Command]
    private async void CreateLobby()
    {

        string lobbyName = "mitt lobbynavn";
        int maxPlayers = 4;
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers);
        Debug.Log("Created lobby with lobbycode: " + lobby.LobbyCode);

        hostLobby = lobby;
        Debug.Log("Created Lobby! " + lobby.Name);
    }

    [Command]
    private async void ListLobbies()
    {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        Debug.Log("found " + queryResponse.Results.Count + " lobbies.");
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
        Lobby clientLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
        Debug.Log("hostId of joined lobby" + clientLobby.HostId);
    }
}
