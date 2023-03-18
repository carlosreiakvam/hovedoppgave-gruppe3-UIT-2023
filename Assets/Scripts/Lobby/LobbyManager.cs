using QFSW.QC;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
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
        if (isActive)
        {
            HandleLobbyHeartBeat();
            HandlePollUpdate();
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, joinedLobby.Players[1].Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

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
        catch (Exception e)
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


    private async void HandlePollUpdate()
    {
        float LobbyPollTimerMax = 1.1f; // rate limit of one request per second

        if (joinedLobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                try
                {
                    lobbyUpdateTimer = LobbyPollTimerMax;
                    Debug.Log("sending heartbeat");
                    Lobby lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                    joinedLobby = lobby;

                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
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
            joinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            Debug.Log("hostId of joined lobby" + joinedLobby.HostId);
            Debug.Log("Joined lobby: " + playerName);
            PrintPlayers(joinedLobby);
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
            PrintPlayers(quickLobby);
        }
        catch (LobbyServiceException e)
        { Debug.Log(e); }

    }

    private void PrintPlayers(Lobby lobby)
    {
        try
        {
            Debug.Log("Listing players in " + lobby.Name);

            foreach (Player player in lobby.Players)
            {
                Debug.Log(player.Data["PlayerName"].Value);

            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
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
