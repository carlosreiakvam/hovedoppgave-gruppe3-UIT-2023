using QFSW.QC;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
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
    private int screenHeight;


    //  Move to center on screen resize
    private void Update()
    {
        if (Screen.height != screenHeight)
        {
            screenHeight = Screen.height;
            transform.position = new Vector3(Screen.width / 2, screenHeight / 2, 0f);
            joinCodeDisplayGO.transform.position = new Vector3(Screen.width / 2, screenHeight - screenHeight * 0.2f, 0f);
        }

    }


    private void Start()
    {
        joinCodeDisplay = joinCodeDisplayGO.GetComponent<TextMeshProUGUI>();
        TMP_InputField joinCodeInput = joinCodeGO.GetComponent<TMP_InputField>();
        Button startHostButton = startHostButtonGO.GetComponent<Button>();
        Button startClientButton = startClientButtonGO.GetComponent<Button>();
        try
        {
            GameSceneShortcut gameSceneShortcut = GameObject.Find("GameSceneShortcut").GetComponent<GameSceneShortcut>();
            bool isCommingFromLobby = !gameSceneShortcut.isShortcutUsed;
            if (isCommingFromLobby)
            {
                transform.localScale = Vector3.zero;
                return;
            }
        }
        catch { Debug.Log("Not comming from lobby"); }

        Instantiate(NetworkManagerPrefab);
        //Instantiate(chatVisualPrefab);


        startHostButton.onClick.AddListener(() =>
        {
            StartHost();
            transform.localScale = Vector3.zero;
        });

        startClientButton.onClick.AddListener(() =>
        {
            StartClient(joinCodeText.text);
            transform.localScale = Vector3.zero;
        });

    }


    [Command]
    public async void StartHost()
    {
        RelayManager RelayManager = relayManagerGO.GetComponent<RelayManager>();
        try
        {
            await RelayManager.Authorize();
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
            await RelayManager.Authorize();
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
