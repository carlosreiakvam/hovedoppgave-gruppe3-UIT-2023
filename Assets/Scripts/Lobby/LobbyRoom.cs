using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyRoom : MonoBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] Button leaveButton;
    [SerializeField] Button readyButton;
    [SerializeField] Button startGameButton;
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] List<GameObject> playerNameFields;
    [SerializeField] List<GameObject> playerReadyFields;
    [SerializeField] List<PlayerNameSO> playerNamesSO = new(4);

    bool areAllPlayersReady = false;
    bool isLocalPlayerReady = false;
    bool isGameInitiated = false;

    List<TextMeshProUGUI> playerNamesText = new();


    private void Awake()
    {
        HandleReadyButton();
        HandleStartGameButton();
        HandleLeaveButton();
    }
    private void Start()
    {
        MenuManager.Singleton.OnLobbyRoomOpened += MenuManager_OnLobbyRoomOpened;
    }


    private void MenuManager_OnLobbyRoomOpened(object sender, EventArgs e)
    {
        InitPlayers();
        SetHeader();
        Subscribe();
    }


    private void InitPlayers()
    {

        // Fill the playerNamesText list with the player names.
        foreach (GameObject playerName in playerNameFields)
        {
            TextMeshProUGUI playerNameText = playerName.GetComponent<TextMeshProUGUI>();
            playerNamesText.Add(playerNameText);
        }

        // Fill the player ready text list 
        foreach (GameObject playerReady in playerReadyFields)
        {
            TextMeshProUGUI playerReadyText = playerReady.GetComponent<TextMeshProUGUI>();
            playerNamesText.Add(playerReadyText);
        }


        // Deactivate all player and ready objects
        foreach (GameObject p in playerNameFields) { p.SetActive(false); }
        foreach (GameObject p in playerReadyFields) { p.SetActive(false); }
    }

    /// <summary>
    /// Connects the LobbyRoom with MenuManager and LobbyManager.
    /// </summary>
    private void Subscribe()
    {
        LobbyManager.Singleton.OnHandlePollUpdate += HandlePollUpdate;
        LobbyManager.Singleton.OnLobbyLeft += OnLobbyLeft;
    }


    /// <summary>
    /// Handles the functionality of the ready button.
    /// </summary>
    private void HandleReadyButton()
    {
        TextMeshProUGUI readyButtonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        readyButton.onClick.AddListener(() =>
        {
            isLocalPlayerReady = !isLocalPlayerReady;
            string readyStatus = isLocalPlayerReady ? LobbyStringConst.TRUE : LobbyStringConst.FALSE;
            LobbyManager.Singleton.SetPlayerReady(readyStatus);
            readyButtonText.text = isLocalPlayerReady ? "Not ready" : "Ready";
        });
    }

    private void HandleStartGameButton()
    {
        startGameButton.interactable = false;
        startGameButton.onClick.AddListener(() => { LobbyManager.Singleton.QueGameStart(); });
    }
    private void HandleLeaveButton()
    {
        leaveButton.onClick.AddListener(() => { LobbyManager.Singleton.RequestLeaveLobby(); });
    }

    /// <summary>
    /// Sets the lobby name and code text.
    /// </summary>
    private void SetHeader()
    {
        header.text = LobbyManager.Singleton.lobbyName;
    }


    /// <summary>
    /// Fired by OnHandlePollUpdate from LobbyManager.
    /// Handles lobby updates based on the lobby polling event.
    /// </summary>
    public void HandlePollUpdate(object sender, EventArgs lobbyEventArg)
    {
        // Get remote lobby as event argument. This happens every second.
        var lobbyEventArgs = lobbyEventArg as LobbyEventArgs;
        var lobby = lobbyEventArgs.Lobby;
        if (lobby == null) { MenuManager.Singleton.OpenAlert("Lobby connection lost"); }
        UpdateLocalLobby(lobby);
    }


    /// <summary>
    /// Updates local lobby data.
    /// Called by LobbyManager.
    /// </summary>
    public void UpdateLocalLobby(Lobby lobby)
    {
        if (isGameInitiated) return;
        areAllPlayersReady = true; // will be false if any player is not ready
        bool isCurrentPlayerHost = false;
        string thisPlayerId = "";
        string lobbyHostId = lobby.Data[LobbyStringConst.HOST_ID].Value;

        // Allways assume that all players left the lobby
        for (int j = 0; j < 4; j++)
        {
            playerNameFields[j].SetActive(false);
        }

        int i = 0;
        foreach (Player player in lobby.Players)
        {
            // Get player names position in the list
            Vector2 playerNamePos = playerNameFields[i].transform.position;
            Vector2 playerReadyPos = playerNamePos;
            playerReadyPos.x += 400;
            playerReadyFields[i].transform.position = playerReadyPos;

            // Activate player object 
            playerNameFields[i].SetActive(true);

            // Check if current player is ready
            bool isPlayerReady = player.Data[LobbyStringConst.IS_PLAYER_READY].Value == LobbyStringConst.TRUE;
            playerReadyFields[i].SetActive(isPlayerReady);

            // If any player is not ready, areAllPlayersReady is false 
            if (!isPlayerReady) areAllPlayersReady = false;

            // Set thisPlayer based on this authorized instance
            if (player.Data[LobbyStringConst.PLAYER_ID].Value.Equals(LobbyManager.Singleton.GetThisPlayerId()))
            {
                thisPlayerId = player.Data[LobbyStringConst.PLAYER_ID].Value;
            }

            // Check if current player is host
            isCurrentPlayerHost = thisPlayerId.Equals(lobbyHostId);

            // Set name of current player
            playerNamesText[i].text = player.Data[LobbyStringConst.PLAYER_NAME].Value;

            // Set name to the scriptable object
            playerNamesSO[i].SetValue(player.Data[LobbyStringConst.PLAYER_NAME].Value);

            i++;
        }

        // Start button is only interactable if the current player is host and all players are ready

        startGameButton.interactable = (areAllPlayersReady && isCurrentPlayerHost);


        // If the game is set as ready from the lobby object, load the network
        if (lobby.Data[LobbyStringConst.IS_LOBBY_READY].Value == LobbyStringConst.TRUE)
        {
            if (!isGameInitiated)
            {
                isGameInitiated = true;

                // Stop the lobby polling so that the lobby is not updated anymore
                LobbyManager.Singleton.StopLobbyPolling();

                // Only the host loads the network
                if (isCurrentPlayerHost)
                {
                    LoadNetwork(isCurrentPlayerHost);
                    LobbyManager.Singleton.SpawnTransitionHelper();

                    // Destroy lobby gracefully
                    LobbyManager.Singleton.EndLobby();
                }
            }
        }
    }



    /// <summary>
    /// Prepares the NetworkManager for game start based on the host status.
    /// </summary>
    private void LoadNetwork(bool isHost)
    {
        if (isHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(
        NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        connectionApprovalResponse.Approved = true;
    }


    /// <summary>
    /// Handles the lobby leave event.
    /// </summary>
    public void OnLobbyLeft(object sender, EventArgs e)
    {
        LobbyManager.Singleton.EndLobby(); // only invoked by server
        MenuManager.Singleton.OpenPage(MenuEnums.LobbyMenu);
    }
}
