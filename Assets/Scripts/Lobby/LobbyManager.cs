using QFSW.QC;
using System;
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
    private string playerName;

    private void Start()
    {
        playerName = "ape_" + UnityEngine.Random.Range(10, 99);
    }

    public async void OnOpenMenu()
    {
        Debug.Log("player name: " + playerName);
        try
        {
            isActive = true;
            Debug.Log("OnOpenMenu");
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
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
                try
                {
                    heartbeatTimer = heartbeatTimerMax;
                    Debug.Log("sending heartbeat");
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    [Command]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 4;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {

                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created Lobby: " + lobby.Name);
            Debug.Log("lobby code: " + lobby.LobbyCode + " lobby id: " + lobby.Id);
            hostLobby = lobby;
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                { new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT) },
                Order = new List<QueryOrder>
                { new QueryOrder(false,QueryOrder.FieldOptions.Created) }
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Debug.Log("found " + queryResponse.Results.Count + " lobbies.");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    [Command]
    private async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            Lobby clientLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("hostId of joined lobby" + clientLobby.HostId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);

        }
    }

    [Command]
    private async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions
            {
                Player = GetPlayer()
            };
            Lobby quickLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            Debug.Log("hostId of joined lobby" + quickLobby.HostId);
        }
        catch (LobbyServiceException e)
        { Debug.Log(e); }

    }

    private void PrintPlayers(Lobby lobby)
    {
        foreach (Player player in lobby.Players)
        {
            Debug.Log("player in " + lobby.Name + ": " + player.Data["PlayerName"].Value);

        }

    }

    private Player GetPlayer()
    {
        return new Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)}}
        };
    }
}
