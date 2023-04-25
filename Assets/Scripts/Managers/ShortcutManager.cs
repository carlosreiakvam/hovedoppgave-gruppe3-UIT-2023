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
    [SerializeField] ShortcutMenu shortcutMenu;
    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] GameObject startHostButtonGO;
    [SerializeField] GameObject startClientButtonGO;
    [SerializeField] TextMeshProUGUI joinCodeInput;
    [SerializeField] GameObject joinCodeDisplayGO;
    TMP_InputField joinCodeDisplayText;
    private int screenHeight;

    public static ShortcutManager Singleton;
    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        joinCodeDisplayText = joinCodeDisplayGO.GetComponent<TMP_InputField>();
    }


    private void OnEnable()
    {
        if (!IsShortcutManagerRelevant()) return;
        Debug.LogWarning("SHORTCUT IN USE");
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        AddButtonListeners();
    }

    private void AddButtonListeners()
    {
        Button startHostButton = startHostButtonGO.GetComponent<Button>();
        Button startClientButton = startClientButtonGO.GetComponent<Button>();

        startHostButton.onClick.AddListener(() =>
        {
            StartHostShortcut();
            shortcutMenu.closeMenu();
        });

        startClientButton.onClick.AddListener(() =>
        {
            Debug.Log(joinCodeInput.text);
            StartClientShortcut(joinCodeInput.text);
            shortcutMenu.closeMenu();
        });
    }


    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        Debug.Log("ONCLIENTCONNECTEDALLBACK");
        SpawnManager.Singleton.SpawnAllPlayers();
    }


    public async void StartHostShortcut()
    {
        try
        {
            await RelayManager.Singleton.Authorize();
            Dictionary<LobbyEnums, string> relaydict = await RelayManager.Singleton.CreateRelayShortcut();
            string joincode = relaydict[LobbyEnums.RelayJoinCode];
            joinCodeDisplayText.text = joincode;
        }
        catch (Exception e) { Debug.Log(e); }

        SpawnManager.Singleton.SpawnAllPrefabs();

    }

    public async void StartClientShortcut(string joincode)
    {

        try
        {
            await RelayManager.Singleton.Authorize();
            await RelayManager.Singleton.JoinRelayShortcut(joincode);
            joinCodeDisplayGO.SetActive(false);

        }
        catch (Exception e) { Debug.LogError(e); }

        try
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        }
        catch (Exception e)
        { Debug.LogWarning("CustomNetworkManager Connection error: " + e); }

    }


    private void NetworkManager_ConnectionApprovalCallback(
    NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
    NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        connectionApprovalResponse.Approved = true;
    }

    private bool IsShortcutManagerRelevant()
    {
        // return if shortcut not used or networkmanager not initialized
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("Do not attempt to start game from game scene. NetworkManager is not initialized");
            return false;
        }
        return true;
    }


}
