using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;

using Unity.Services.Authentication;
using System.Threading.Tasks;

public class LobbyPreGame : MonoBehaviour //NetworkBehaviour
{
    //[SerializeField] GameObject networkManagerGO;

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
    
    //[SerializeField] private GameObject playerPrefab;
    bool isReady = false;
    bool isGameReady = false;
    //NetworkManager relayConnector;


    Button readyButton;
    MenuManager menuManager;
    Button leaveButton;
    Button startGameButton;
    TextMeshProUGUI readyButtonText;
    List<Toggle> isReadyStates;
    List<TextMeshProUGUI> pNames;
    GameObject[] pNamesGO;


    /* Tore experimentation*/
    //public event EventHandler OnPlayerDataNetworkListChanged;
    //private NetworkList<PlayerData> playerDataNetworkList;
    //private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
    //private string playerName;
    //private Dictionary<ulong, bool> playerReadyDictionary;
    //public static LobbyPreGame Instance { get; private set; }
    //public event EventHandler OnReadyChanged;
    /*--------------------*/


    void Start()
    {
        GameObject parent = this.transform.parent.gameObject;
        menuManager = menuManagerGO.GetComponent<MenuManager>();

        // Relay connector

        //relayConnector = networkManagerGO.GetComponent<HeroNetworkManager.NetworkManager>();


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



    /* Tore experimentation*/
    //private void Awake()
    //{
    //    Instance = this;

    //    playerReadyDictionary = new Dictionary<ulong, bool>();

    //    playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));

    //    playerDataNetworkList = new NetworkList<PlayerData>();
    //    playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
    //}

    //private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    //{
    //    OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    //}

    /*--------------------*/



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


    //public void SetPlayerReady()
    //{
    //    SetPlayerReadyServerRpc();
    //}

    //[ServerRpc(RequireOwnership = false)]
    //private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    //{
    //    SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

    //    playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

    //    bool allClientsReady = true;
    //    foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
    //    {
    //        if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
    //        {
    //            // This player is NOT ready
    //            allClientsReady = false;
    //            break;
    //        }
    //    }

    //    if (allClientsReady)
    //    {
    //        //KitchenGameLobby.Instance.DeleteLobby();
    //        LoadNetwork(true);
    //    }
    //}

    //[ClientRpc]
    //private void SetPlayerReadyClientRpc(ulong clientId)
    //{
    //    playerReadyDictionary[clientId] = true;

    //    OnReadyChanged?.Invoke(this, EventArgs.Empty);
    //}

    //private bool StartGame(bool isHost)
    //{
    //    bool connected;
    //    if (isHost) { connected = relayConnector.StartHost(); }
    //    else { connected = relayConnector.StartClient(); }
    //    return connected;
    //}





    private void LoadNetwork(bool isHost)
    {
        if (isHost)
        {

            Unity.Netcode.NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        }
    }

    private void NetworkManager_ConnectionApprovalCallback(Unity.Netcode.NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, Unity.Netcode.NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //    if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        //    {
        //        connectionApprovalResponse.Approved = false;
        //        connectionApprovalResponse.Reason = "Game has already started";
        //        return;
        //    }

        //    if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
        //    {
        //        connectionApprovalResponse.Approved = false;
        //        connectionApprovalResponse.Reason = "Game is full";
        //        return;
        //    }

        connectionApprovalResponse.Approved = true;
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
