//using ParrelSync;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyEventArgs : EventArgs
{
    public Lobby Lobby { get; set; }
}

/// <summary>
/// The LobbyManager class handles all the lobby related functionality such as lobby creation, lobby joining, lobby updating, and player management within the lobby.
/// It also handles lobby polling, which is a method of regularly checking the state of the lobby.
/// </summary>
public class LobbyManager : NetworkBehaviour
{

    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private int maxPlayers = 4;
    private Lobby lobby;

    [SerializeField] GameObject transitionHelperPrefab;
    [SerializeField] GameStatusSO gameStatusSO;
    [HideInInspector] public string lobbyName;
    [HideInInspector] public string lobbyCode;
    [HideInInspector] public bool isHost = false;
    private bool isLobbyActive = false;

    public event EventHandler OnHandlePollUpdate;  // lets LobbyRoom know when lobby is created
    public event EventHandler OnLobbyLeft;
    public static LobbyManager Singleton;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
            // Class is not kept alive on scene change
        }
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


    /// <summary>
    /// Called regularly to get the current state of the lobby from the server.
    /// </summary>
    private async void HandlePollUpdate()
    {
        float LobbyPollTimerMax = 1.1f; // rate limit of one request per 1.1 second

        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer < 0f)
        {
            lobbyUpdateTimer = LobbyPollTimerMax;
            lobby = await LobbyService.Instance.GetLobbyAsync(lobby.Id);
            if (lobby != null) OnHandlePollUpdate.Invoke(this, new LobbyEventArgs { Lobby = lobby });
            else MenuManager.Singleton.OpenAlert("Lobby not located");
        }
    }

    /// <summary>
    /// Sets the lobby to be ready to start the game.
    /// </summary>
    public async void QueGameStart()
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                     LobbyStringConst.IS_LOBBY_READY,  new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: LobbyStringConst.TRUE)
                },
            };

        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);

    }

    /// <summary>
    /// Called regularly to send a heartbeat to the server to keep the lobby alive.
    /// </summary>
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
                    await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new lobby.
    /// </summary>
    public async Task CreateLobby(string lobbyName, bool isPrivate, string playerName)
    {
        try
        {

            await RelayManager.Singleton.Authorize();
            this.lobbyName = lobbyName;
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetNewPlayer(playerName)
            };
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                LobbyStringConst.IS_LOBBY_READY, new DataObject(
                 visibility: DataObject.VisibilityOptions.Public, // Visible publicly.
                 value: LobbyStringConst.FALSE)
                 },
            };

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            if (lobby == null) Debug.LogWarning("Lobby not initiated");
            SetHostId(AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e) { Debug.LogWarning("Unable to create lobby"); Debug.LogError(e); }

        try
        {
            // Connect to relay
            Dictionary<string, string> relayValues = await RelayManager.Singleton.CreateRelay();
            UpdateJoinCode(relayValues[LobbyStringConst.RELAY_JOIN_CODE]);
            Debug.Log($"CreateLobby RelayJoinCode: {relayValues[LobbyStringConst.RELAY_JOIN_CODE]}");
            isLobbyActive = true;
        }
        catch (Exception e) { Debug.Log("Relay Connector error: " + e); }
    }

    /// <summary>
    /// Updates the relay join code for the lobby.
    /// </summary>
    public async void UpdateJoinCode(string joinCode)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    LobbyStringConst.RELAY_JOIN_CODE, new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: joinCode)
                },
            };


        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }


    public async Task<bool> QuickJoinLobby(string playerName)
    {
        try
        {
            // Attempt to get lobby
            await RelayManager.Singleton.Authorize();

            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions { Player = GetNewPlayer(playerName) };
            lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            isLobbyActive = true;

            // Connect to relay
            string relayCode = lobby.Data[LobbyStringConst.RELAY_JOIN_CODE].Value;
            Debug.Log($"QuickJoin RelayJoinCode: {relayCode}");
            RelayManager.Singleton.JoinRelay(relayCode);

            return true;
        }
        catch { return false; }

    }
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

    /// <summary>
    /// Returns a new player object.
    /// </summary>
    private Player GetNewPlayer(string playerName)
    {
        return new Player()
        {
            Data = new Dictionary<string, PlayerDataObject>
                {
                    {LobbyStringConst.PLAYER_ID , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, AuthenticationService.Instance.PlayerId )},
                    {LobbyStringConst.PLAYER_NAME , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, playerName)},
                    {LobbyStringConst.IS_PLAYER_READY , new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, LobbyStringConst.FALSE)},
            }
        };
    }

    /// <summary>
    /// Sets the host id for the lobby.
    /// </summary>
    private async void SetHostId(string hostId)
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    LobbyStringConst.HOST_ID, new DataObject(
                        visibility: DataObject.VisibilityOptions.Public,
                        value: hostId)
                },
            };

        await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }


    /// <summary>
    /// Sets the player's ready state.
    /// </summary>
    public async void SetPlayerReady(string readyConst)
    {
        UpdatePlayerOptions options = new UpdatePlayerOptions();
        options.Data = new Dictionary<string, PlayerDataObject>()
            {
                {
                    LobbyStringConst.IS_PLAYER_READY, new PlayerDataObject(
                        visibility: PlayerDataObject.VisibilityOptions.Public,
                        value: readyConst)
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
            OnLobbyLeft.Invoke(this, EventArgs.Empty); // tells LobbyRoom that the lobby is left
        }
        catch (Exception e)
        {
            Debug.Log("RequestLeaveLobby error: " + e);
        }

    }


    /// <summary>
    /// Set the "IsGameReady" data object to true.
    /// </summary>
    public void StartGame()
    {
        UpdateLobbyOptions options = new UpdateLobbyOptions();
        options.Data = new Dictionary<string, DataObject>()
            {
                {
                    LobbyStringConst.IS_PLAYER_READY, new DataObject(
                        visibility: DataObject.VisibilityOptions.Private,
                        value: LobbyStringConst.TRUE)
                },
            };

        LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
    }

    /// <summary>
    /// Returns the id of the current player.
    /// </summary>
    public string GetThisPlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    internal void StopLobbyPolling()
    {
        isLobbyActive = false;
    }

    /// <summary>
    /// Spawns a helper object for transitioning between scenes.
    /// The reason for spawning this is so that it can send RPC messages
    /// </summary>
    public void SpawnTransitionHelper()
    {
        // only run by host
        GameObject instance = Instantiate(transitionHelperPrefab);
        NetworkObject networkObject = instance.GetComponent<NetworkObject>();
        networkObject.Spawn();
    }

    /// <summary>
    /// Registers the lobby information to a ScriptableObject representing the game state.
    /// </summary>
    internal void RegisterLobbyToGameStatusSO()
    {
        gameStatusSO.lobbyPlayers = lobby.Players;
    }
}
