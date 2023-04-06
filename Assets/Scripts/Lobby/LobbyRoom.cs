using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyRoom : MonoBehaviour //NetworkBehaviour
{

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

    bool isReady = false;
    bool isGameReady = false;
    bool isGameInitiated = false;

    private LobbyManager lobbyManager;

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

        // Connect with managers
        menuManager = GetComponentInParent<MenuManager>();
        lobbyManager = GetComponentInParent<LobbyManager>();
        lobbyManager.OnHandlePollUpdate += HandlePollUpdate;
        lobbyManager.OnLobbyLeft += OnLobbyLeft;

        // Handle toggles
        Toggle p1Toggle = p1ToggleGO.GetComponent<Toggle>();
        Toggle p2Toggle = p2ToggleGO.GetComponent<Toggle>();
        Toggle p3Toggle = p3ToggleGO.GetComponent<Toggle>();
        Toggle p4Toggle = p4ToggleGO.GetComponent<Toggle>();
        isReadyStates = new List<Toggle> { p1Toggle, p2Toggle, p3Toggle, p4Toggle };

        // Player names
        pNames = new List<TextMeshProUGUI> { p1Name, p2Name, p3Name, p4Name };
        pNamesGO = new GameObject[] { p1, p2, p3, p4 };
        foreach (GameObject playerName in pNamesGO) { playerName.SetActive(false); }

        // Ready button
        readyButton = readyButtonGO.GetComponent<Button>();
        readyButtonText = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        readyButton.onClick.AddListener(() =>
        {
            isReady = !isReady;
            lobbyManager.SetPlayerReady(isReady);
            readyButtonText.text = isReady ? "Not ready" : "Ready";
        });

        // Start game button
        startGameButton = startGameButtonGO.GetComponent<Button>();
        startGameButton.interactable = false;
        startGameButtonGO.SetActive(false);
        startGameButton.onClick.AddListener(() => { lobbyManager.QueGameStart(); });

        // Leave button
        leaveButton = leaveButtonGO.GetComponent<Button>();
        leaveButton.onClick.AddListener(() => { lobbyManager.RequestLeaveLobby(); });

        lobbyNameText.text = lobbyManager.lobbyName;
        lobbyCodeText.text = lobbyManager.lobbyCode;

    }

    public void HandlePollUpdate(object sender, EventArgs e)
    {
        // Get remote lobby as event argument. This happens every second.
        var lobbyEventArgs = e as LobbyEventArgs;
        var lobby = lobbyEventArgs.Lobby;
        UpdateLocalLobby(lobby);
    }


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
            if (player.Data[LobbyEnums.PlayerId.ToString()].Value.Equals(lobbyManager.GetThisPlayerId()))
            {
                thisPlayerId = player.Data[LobbyEnums.PlayerId.ToString()].Value;
            }

            // Check if authenticated player is host of this lobby
            authenticatedIsHost = thisPlayerId.Equals(lobbyHostId);
            //thisPlayerIsHost = player.Data[LobbyEnums.PlayerId.ToString()].Value.Equals(lobbyHostId)

            // if player of current iteration is host
            if (player.Data[LobbyEnums.PlayerId.ToString()].Value.Equals(lobbyHostId))
            { pNames[i].text = player.Data["PlayerName"].Value + " [host]"; }
            else { pNames[i].text = player.Data["PlayerName"].Value; }

            // Game is not ready if any one of the ready states are false
            if (!isReadyStates[i].isOn) { isGameReady = false; }
            i++;
        }

        if (authenticatedIsHost) { startGameButtonGO.SetActive(true); }
        if (isGameReady) { startGameButton.interactable = true; }
        if (lobby.Data["IsGameReady"].Value == true.ToString())
        {
            if (!isGameInitiated)
            {
                isGameInitiated = true;
                LoadNetwork(authenticatedIsHost);
                //menuManager.DestroyMenus();
            }
        }
    }



    private void LoadNetwork(bool isHost)
    {
        if (isHost)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(Unity.Netcode.NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, Unity.Netcode.NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {

        connectionApprovalResponse.Approved = true;
    }


    public void OnLobbyLeft(object sender, EventArgs e)
    {
        menuManager.OpenPage(MenuEnums.LobbyMenu);
    }
}
