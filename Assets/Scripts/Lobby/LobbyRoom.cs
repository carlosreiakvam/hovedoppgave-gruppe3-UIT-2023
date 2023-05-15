using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyRoom : MonoBehaviour
{
    public const string PLAYER_NAME = "PlayerName";
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject leaveButtonGO;
    [SerializeField] GameObject readyButtonGO;
    [SerializeField] GameObject startGameButtonGO;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] TextMeshProUGUI p1Name;
    [SerializeField] TextMeshProUGUI p2Name;
    [SerializeField] TextMeshProUGUI p3Name;
    [SerializeField] TextMeshProUGUI p4Name;
    [SerializeField] GameObject p1;
    [SerializeField] GameObject p2;
    [SerializeField] GameObject p3;
    [SerializeField] GameObject p4;
    [SerializeField] GameObject p1ToggleGO;
    [SerializeField] GameObject p2ToggleGO;
    [SerializeField] GameObject p3ToggleGO;
    [SerializeField] GameObject p4ToggleGO;
    [SerializeField] List<PlayerNameSO> playerNameList = new(4);

    bool isReady = false;
    bool isGameReady = false;
    bool isGameInitiated = false;


    Button readyButton;
    MenuManager menuManager;
    Button leaveButton;
    Button startGameButton;
    TextMeshProUGUI readyButtonText;
    List<Toggle> isReadyStates;
    List<TextMeshProUGUI> pNames;
    GameObject[] pNamesGO;

    void Start()
    {
        ConnectWithManagers();
        GetToggles();
        InitPlayerNames();
        HandleReadyButton();
        HandleStartGameButton();
        HandleLeaveButton();
        SetLobbyText();
    }


    /// <summary>
    /// Connects the LobbyRoom with MenuManager and LobbyManager.
    /// </summary>
    private void ConnectWithManagers()
    {
        menuManager = GetComponentInParent<MenuManager>();
        LobbyManager.Singleton.OnHandlePollUpdate += HandlePollUpdate;
        LobbyManager.Singleton.OnLobbyLeft += OnLobbyLeft;
    }

    /// <summary>
    /// Retrieves the toggles for player ready states from GameObjects.
    /// </summary>
    private void GetToggles()
    {
        Toggle p1Toggle = p1ToggleGO.GetComponent<Toggle>();
        Toggle p2Toggle = p2ToggleGO.GetComponent<Toggle>();
        Toggle p3Toggle = p3ToggleGO.GetComponent<Toggle>();
        Toggle p4Toggle = p4ToggleGO.GetComponent<Toggle>();
        isReadyStates = new List<Toggle> { p1Toggle, p2Toggle, p3Toggle, p4Toggle };
    }

    /// <summary>
    /// Initializes player name GameObjects and sets their active status to false.
    /// </summary>
    private void InitPlayerNames()
    {
        pNames = new List<TextMeshProUGUI> { p1Name, p2Name, p3Name, p4Name };
        pNamesGO = new GameObject[] { p1, p2, p3, p4 };
        foreach (GameObject playerName in pNamesGO) { playerName.SetActive(false); }
    }

    /// <summary>
    /// Handles the functionality of the ready button.
    /// </summary>
    private void HandleReadyButton()
    {
        readyButton = readyButtonGO.GetComponent<Button>();
        readyButtonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        readyButton.onClick.AddListener(() =>
        {
            isReady = !isReady;
            LobbyManager.Singleton.SetPlayerReady(isReady);
            readyButtonText.text = isReady ? "Not ready" : "Ready";
        });
    }

    private void HandleStartGameButton()
    {
        startGameButton = startGameButtonGO.GetComponent<Button>();
        startGameButton.interactable = false;
        startGameButtonGO.SetActive(false);
        startGameButton.onClick.AddListener(() => { LobbyManager.Singleton.QueGameStart(); });
    }
    private void HandleLeaveButton()
    {
        leaveButton = leaveButtonGO.GetComponent<Button>();
        leaveButton.onClick.AddListener(() => { LobbyManager.Singleton.RequestLeaveLobby(); });
    }

    /// <summary>
    /// Sets the lobby name and code text.
    /// </summary>
    private void SetLobbyText()
    {
        lobbyNameText.text = LobbyManager.Singleton.lobbyName;
        lobbyCodeText.text = LobbyManager.Singleton.lobbyCode;
    }


    /// <summary>
    /// Handles lobby updates based on the lobby polling event.
    /// </summary>
    public void HandlePollUpdate(object sender, EventArgs e)
    {
        // Get remote lobby as event argument. This happens every second.
        var lobbyEventArgs = e as LobbyEventArgs;
        var lobby = lobbyEventArgs.Lobby;
        UpdateLocalLobby(lobby);
    }


    /// <summary>
    /// Updates local lobby data based on the state of the remote lobby.
    /// </summary>
    public void UpdateLocalLobby(Lobby lobby)
    {
        isGameReady = true; // will be false if any player is not ready
        bool authenticatedIsHost = false;
        string thisPlayerId = "";
        string lobbyHostId = lobby.Data[LobbyEnums.HostId.ToString()].Value;

        for (int j = 0; j < 4; j++)
        { pNamesGO[j].SetActive(false); }

        int i = 0;
        foreach (Player player in lobby.Players)
        {
            // Activate player object and set name
            pNamesGO[i].SetActive(true);

            // Set ready state
            isReadyStates[i].isOn = (player.Data["IsReady"].Value == true.ToString());

            // Set thisPlayer based on this authorized instance
            if (player.Data[LobbyEnums.PlayerId.ToString()].Value.Equals(LobbyManager.Singleton.GetThisPlayerId()))
            {
                thisPlayerId = player.Data[LobbyEnums.PlayerId.ToString()].Value;
            }

            // Check if authenticated player is host of this lobby
            authenticatedIsHost = thisPlayerId.Equals(lobbyHostId);

            // if player of current iteration is host
            if (player.Data[LobbyEnums.PlayerId.ToString()].Value.Equals(lobbyHostId))
            {
                pNames[i].text = player.Data[PLAYER_NAME].Value + " [host]";
            }
            else
            {
                pNames[i].text = player.Data[PLAYER_NAME].Value;
            }

            // Game is not ready if any one of the ready states are false
            if (!isReadyStates[i].isOn) { isGameReady = false; }


            //playerNameList[i].PlayerName = player.Data[PLAYER_NAME].Value;
            playerNameList[i].SetValue(player.Data[PLAYER_NAME].Value);

            i++;

        }

        if (authenticatedIsHost) { startGameButtonGO.SetActive(true); }
        if (isGameReady) { startGameButton.interactable = true; }
        if (lobby.Data["IsGameReady"].Value == true.ToString())
        {
            if (!isGameInitiated)
            {
                isGameInitiated = true;
                LobbyManager.Singleton.StopLobbyPolling(); 

                if (authenticatedIsHost)
                {
                    LoadNetwork(authenticatedIsHost);
                    LobbyManager.Singleton.SpawnTransitionHelper();
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
        menuManager.OpenPage(MenuEnums.LobbyMenu);
    }
}
