using QFSW.QC;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShortcutManager : NetworkBehaviour
{
    [SerializeField] NetworkManager NetworkManagerPrefab;
    [SerializeField] GameObject chatVisualPrefab;
    [SerializeField] Transform playerPrefab;
    [SerializeField] GameObject relayManagerGO;
    [SerializeField] GameObject startHostButtonGO;
    [SerializeField] GameObject startClientButtonGO;
    [SerializeField] GameObject joinCodeGO;
    [SerializeField] GameObject joinCodeDisplayGO;
    [SerializeField] TextMeshProUGUI joinCodeText;
    TextMeshProUGUI joinCodeDisplay;
    ChatManager chatManager;

    private void Start()
    {
        joinCodeDisplay = joinCodeDisplayGO.GetComponent<TextMeshProUGUI>();
        TMP_InputField joinCodeInput = joinCodeGO.GetComponent<TMP_InputField>();
        Button startHostButton = startHostButtonGO.GetComponent<Button>();
        Button startClientButton = startClientButtonGO.GetComponent<Button>();

        bool isGameStartedFromLobby = NetworkManager != null;
        if (isGameStartedFromLobby)
        {
            joinCodeInput.interactable = false;
            startHostButton.interactable = false;
            startClientButton.interactable = false;
            return; // OPT OUT IF COMMING FROM LOBBY
        }

        Instantiate(NetworkManagerPrefab);
        Instantiate(chatVisualPrefab);


        startHostButton.onClick.AddListener(() =>
        {
            StartHost();
            startHostButton.interactable = false;
            startClientButton.interactable = false;
            joinCodeInput.interactable = false;

        });

        startClientButton.onClick.AddListener(() =>
        {
            StartClient(joinCodeText.text);
            startHostButton.interactable = false;
            startClientButton.interactable = false;
            joinCodeInput.interactable = false;
        });

    }


    [Command]
    public async void StartHost()
    {
        RelayManager RelayManager = relayManagerGO.GetComponent<RelayManager>();
        try
        {
            await RelayManager.InitAndAuthorize();
            Dictionary<LobbyEnums, string> relaydict = await RelayManager.CreateRelayShortcut();
            string joincode = relaydict[LobbyEnums.RelayJoinCode];
            joinCodeDisplay.text = joincode;
        }
        catch (Exception e) { Debug.Log("RelayManager Connection error\n" + e); }
        try
        {
            NetworkManager.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        }
        catch (Exception e)
        { Debug.LogWarning("NetworkManager Connection error: " + e); }
    }

    [Command]
    public async void StartClient(string joincode)
    {
        RelayManager RelayManager = relayManagerGO.GetComponent<RelayManager>();

        try
        {
            await RelayManager.InitAndAuthorize();
            await RelayManager.JoinRelayShortcut(joincode);
        }
        catch (Exception e) { Debug.LogError(e); }

        try
        {
            NetworkManager.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        }
        catch (Exception e)
        { Debug.LogWarning("NetworkManager Connection error: " + e); }

    }


    private void NetworkManager_ConnectionApprovalCallback(
    NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
    NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        connectionApprovalResponse.Approved = true;
    }


}
