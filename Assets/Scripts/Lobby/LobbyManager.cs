//using ParrelSync;
using QFSW.QC;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyEventArgs : EventArgs
{
    public Lobby Lobby { get; set; }
}


public class LobbyManager : MonoBehaviour
{
    MenuManager menuManager;
    [SerializeField] GameObject relayManagerGO;
    RelayManager relayManager;

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private int maxPlayers = 4;

    [HideInInspector] public string lobbyName;
    [HideInInspector] public string lobbyCode;
    [HideInInspector] public string lobbyId;
    [HideInInspector] public bool isHost = false;
    private bool isLobbyActive = false;

    public event EventHandler OnHandlePollUpdate;  // lets LobbyRoom know when lobby is created
    public event EventHandler OnLobbyLeft;


    private void Start()
    {
        menuManager = GetComponent<MenuManager>();
        relayManager = relayManagerGO.GetComponent<RelayManager>();
    }

    private void Update()
    {

        if (isLobbyActive)
        {
            HandlePollUpdate();
            if (isHost)
            {
                HandleLobbyHeartBeat();
            }
        }
    }


    private async void HandlePollUpdate()
    {
        Lobby lobby;
        float LobbyPollTimerMax = 1.1f; // rate limit of one request per 1.1 second

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0f)
        {
            lobbyUpdateTimer = LobbyPollTimerMax;
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
                if (lobby != null) OnHandlePollUpdate.Invoke(this, new LobbyEventArgs { Lobby = lobby });
            }
            catch { Debug.Log("Lobby not located. Trying again next PollUpdate"); }
        }
    }

    public async void QueGameStart()
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                     "IsGameReady",  new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: true.ToString())
                },
            };

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);

    }

    private async void HandleLobbyHeartBeat()
    {
        float heartbeatTimerMax = 15;

        if (isLobbyActive)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                try
                {
                    heartbeatTimer = heartbeatTimerMax;
                    await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
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
        Lobby lobby;
        try
        {
            await relayManager.Authorize();
            this.lobbyName = lobbyName;
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
            lobbyId = lobby.Id;
            isLobbyActive = true;
            isHost = true;
            lobbyCode = lobby.LobbyCode;
            SetHostId(AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e) { Debug.LogWarning("Unable to create lobby"); Debug.LogError(e); }

        try
        {
            // Connect to relay
            Dictionary<string, string> relayValues = await RelayManager.Instance.CreateRelay();
            UpdateJoinCode(relayValues[LobbyEnums.RelayJoinCode.ToString()]);
            Debug.Log($"CreateLobby RelayJoinCode: {relayValues[LobbyEnums.RelayJoinCode.ToString()]}");
        }
        catch (Exception e) { Debug.Log("Relay Connector error: " + e); }
    }

    public async void UpdateJoinCode(string joinCode)
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


        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
    }


    public async void JoinLobbyByCode(string lobbyCode, string playerName)
    {
        //playerName = GetSemiUniqueName(playerName);
        await relayManager.Authorize();
        JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions { Player = GetNewPlayer(playerName) };

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);

            if (lobby != null)
            {
                isLobbyActive = true;
                lobbyName = lobby.Name;
                lobbyCode = lobby.LobbyCode;
                menuManager.OpenPage(MenuEnums.LobbyRoom);
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
        Lobby lobby;
        try
        {
            // Attempt to get lobby
            //playerName = GetSemiUniqueName(playerName);
            await relayManager.Authorize();

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions { Player = GetNewPlayer(playerName) };
            lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);

            isLobbyActive = true;
            lobbyName = lobby.Name;
            lobbyCode = lobby.LobbyCode;
            lobbyId = lobby.Id;

            // Connect to relay
            string relayCode = lobby.Data[LobbyEnums.RelayJoinCode.ToString()].Value;
            Debug.Log($"QuickJoin RelayJoinCode: {relayCode}");
            RelayManager.Instance.JoinRelay(relayCode);


            // Open LobbyRoom
            menuManager.OpenPage(MenuEnums.LobbyRoom);
        }
        catch (Exception e) { Debug.LogError(e); }

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

        await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
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

        await LobbyService.Instance.UpdatePlayerAsync(lobbyId, AuthenticationService.Instance.PlayerId, options);
    }



    public async void RequestLeaveLobby()
    {
        try
        {
            isLobbyActive = false;
            string playerId = AuthenticationService.Instance.PlayerId;
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            lobbyName = "";
            OnLobbyLeft.Invoke(this, EventArgs.Empty); // tells LobbyRoom that the lobby is left
        }
        catch (Exception e)
        {
            Debug.Log("RequestLeaveLobby error: " + e);
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

        LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
    }

    public string GetThisPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

}
