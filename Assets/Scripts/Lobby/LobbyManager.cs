//using ParrelSync;
using QFSW.QC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject menuManagerGO;
    [SerializeField] GameObject lobbyPreGameGO;
    [SerializeField] GameObject networkManagerGO;
    MenuManager menuManager;
    LobbyPreGame lobbyPreGame;

    public Lobby lobby;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private int maxPlayers = 4;

    private string playerName;
    private string profileName;
    public string lobbyName;
    public string lobbyCode;
    private NetworkManager networkManager;

    public bool isHost = false;
    private bool isLobbyActive = false;

    private void Start()
    {
        playerName = "player";
        menuManager = menuManagerGO.GetComponent<MenuManager>();
        lobbyPreGame = lobbyPreGameGO.GetComponent<LobbyPreGame>();
        networkManager = networkManagerGO.GetComponent<NetworkManager>();
    }

    private void Update()
    {
        if (isLobbyActive)
        {
            HandlePollUpdate();
            if (isHost) { HandleLobbyHeartBeat(); }
        }
    }

    public async Task InitAsyncWithProfile()
    {
        var options = new InitializationOptions();
        profileName = playerName + "_" + UnityEngine.Random.Range(10, 100);
        playerName = profileName;
        options.SetProfile(profileName);
        await UnityServices.InitializeAsync(options);
    }

    private async Task InitAndAuthorize()
    {
        await InitAsyncWithProfile();
        try { if (!AuthenticationService.Instance.IsSignedIn) { await AuthenticationService.Instance.SignInAnonymouslyAsync(); } }
        catch (Exception e) { Debug.Log(e); }
    }



    private async void HandlePollUpdate()
    {
        float LobbyPollTimerMax = 1.1f; // rate limit of one request per 1.1 second

        if (lobby != null)
        {
            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer < 0f)
            {
                try
                {
                    lobbyUpdateTimer = LobbyPollTimerMax;
                    lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);

                    // Probably expensive. Consider changing interval or create conditionals.
                    Debug.Log("update lobby");
                    lobbyPreGame.UpdateFromRemoteLobby(lobby);
                }
                catch (Exception e) { Debug.Log(e); }
            }
        }
    }

    public async void QueGameStart()
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "IsGameReady", new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: true.ToString())
                },
            };

        lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);

    }

    private async void HandleLobbyHeartBeat()
    {
        float heartbeatTimerMax = 15;

        if (lobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                try
                {
                    heartbeatTimer = heartbeatTimerMax;
                    Debug.Log("sending heartbeat");
                    await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    public async Task CreateLobby(string lobbyName, bool isPrivate, string playerName)
    {
        await InitAndAuthorize();
        try
        {
            this.lobbyName = lobbyName;
            this.playerName = playerName;
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetNewPlayer(playerName)
            };
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                "IsGameReady", new DataObject(
                 visibility: DataObject.VisibilityOptions.Public, // Visible publicly.
                 value: false.ToString())
                 },
            };

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            SetHostId(AuthenticationService.Instance.PlayerId);
            isLobbyActive = true;
            isHost = true;
            lobbyCode = lobby.LobbyCode;
        }
        catch (Exception e) { Debug.Log("Unable to create lobby"); Debug.Log(e); }

        try
        {

            Dictionary<string, string> relayValues = await networkManager.CreateRelay();
            UpdateJoinCode(lobby, relayValues[LobbyEnums.RelayJoinCode.ToString()]);
        }
        catch (Exception e) { Debug.Log("Relay Connector error: " + e); }
    }

    public async void UpdateJoinCode(Lobby lobby, string joinCode)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    LobbyEnums.RelayJoinCode.ToString(), new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: joinCode)
                },
            };

        this.lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }





    [Command]
    public async void JoinLobbyByCode(string lobbyCode, string playerName)
    {
        await InitAndAuthorize();
        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions { Player = GetNewPlayer(playerName) };

        try
        {
            Lobby remoteLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            if (remoteLobby != null)
            {
                lobby = remoteLobby;
                isLobbyActive = true;
                lobbyName = lobby.Name;
                lobbyCode = lobby.LobbyCode;
                menuManager.OpenPage(MenuEnums.LobbyPreGame);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            menuManager.OpenAlert("Lobby not found");
        }



    }

    public async void QuickJoinLobby(string playerName)
    {
        try
        {
            await InitAndAuthorize();
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions { Player = GetNewPlayer(playerName) };
            lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            isLobbyActive = true;
            lobbyName = lobby.Name;
            lobbyCode = lobby.LobbyCode;
            menuManager.OpenPage(MenuEnums.LobbyPreGame);

            // Connect to relay
            string relayCode = lobby.Data[LobbyEnums.RelayJoinCode.ToString()].Value;
            networkManager.JoinRelay(relayCode);
        }
        catch (Exception e) { Debug.Log(e); }

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

    private Player GetNewPlayer(string playerName)
    {
        return new Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {"PlayerId" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId )},
                    {"PlayerName" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)},
                    {"IsReady" , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, false.ToString())},
            }
        };
    }

    private async void SetHostId(string hostId)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    LobbyEnums.HostId.ToString(), new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: hostId)
                },
            };

        lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }


    public async void SetPlayerReady(bool ready)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions();
        options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    "IsReady", new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: ready.ToString())
                }
            };

        await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, options);
    }



    public async void RequestLeaveLobby()
    {
        try
        {
            isLobbyActive = false;
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, playerId);
            lobbyName = "";
            playerName = "ape";
            lobbyPreGame.OnLobbyLeft();
        }
        catch (Exception e)
        {
            Debug.Log("RequestLeaveLobby error: " + e);
        }

    }


    private async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, lobby.Players[1].Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    public void StartGame()
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "IsGameReady", new DataObject(
                        visibility: DataObject.VisibilityOptions.Private,
                        value: true.ToString())
                },
            };

        LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }

    public string GetThisPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

}
