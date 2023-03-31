using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

public class LobbyPreGame : MonoBehaviour
{
    [SerializeField] GameObject networkManagerGO;
    [SerializeField] GameObject menuManagerGO;
    [SerializeField] LobbyManager lobbyManager;
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
    NetworkManager relayConnector;

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
        GameObject parent = this.transform.parent.gameObject;
        menuManager = menuManagerGO.GetComponent<MenuManager>();

        // Relay connector
        relayConnector = networkManagerGO.GetComponent<NetworkManager>();

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
    }


    public void OnEnable()
    {
        if (lobbyManager.isActiveAndEnabled)
        {
            lobbyNameText.text = lobbyManager.lobbyName;
            lobbyCodeText.text = lobbyManager.lobby.LobbyCode;
        }
    }

    public void UpdateFromRemoteLobby(Lobby lobby)
    {
        isGameReady = true;
        bool authenticatedIsHost = false;
        string thisPlayerId = "";
        string lobbyHostId = lobby.Data[LobbyEnums.HostId.ToString()].Value;
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
        //if (StartGame(authenticatedIsHost)) 
        //{
        //    StartGame(authenticatedIsHost);
        //}

            LoadNetwork(authenticatedIsHost);
        }
    }

    private bool StartGame(bool isHost)
    {
        bool connected;
        if (isHost) { connected = relayConnector.StartHost(); }
        else { connected = relayConnector.StartClient(); }
        return connected;
    }

    private void LoadNetwork(bool isHost)
    {
        if (isHost)
        {
            NetworkManager.Instance.StartHost();
        }
        else
        {
            NetworkManager.Instance.StartClient();
        }

        Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);

    }

    public void OnLobbyLeft()
    {
        if (menuManager == null)
        {
            Debug.LogWarning("SomeObject is not initialized in OnLobbyLeft");
            return;
        }
        menuManager.OpenPage(MenuEnums.LobbyStart);
    }
}
