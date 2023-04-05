using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HeroNetworkManager;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject menuStartGO;
    [SerializeField] GameObject lobbyStartMenuGO;
    [SerializeField] GameObject lobbyCreateGO;
    [SerializeField] GameObject lobbyPreGameGO;
    [SerializeField] GameObject lobbyQuickJoinGO;
    [SerializeField] GameObject lobbyJoinByCodeGO;
    [SerializeField] GameObject alertMessageGO;
    [SerializeField] GameObject lobbyManagerGO;
    TextMeshProUGUI alertMessage;

    List<GameObject> pages;

    private void Start()
    {
        pages = new List<GameObject> { menuStartGO, lobbyStartMenuGO, lobbyCreateGO, lobbyPreGameGO, lobbyQuickJoinGO, lobbyJoinByCodeGO, alertMessageGO };
        OpenPage(MenuEnums.MenuStart);
        alertMessage = alertMessageGO.GetComponentInChildren<TextMeshProUGUI>();

        HeroNetworkManager.NetworkManager OpenPreGame = HeroNetworkManager.NetworkManager.Instance.GetComponent<NetworkManager>();
        OpenPreGame.OnRelayCreated += OpenLobbyPreGame;

    }

    public void OpenPage(MenuEnums pageToOpen)
    {
        foreach (GameObject page in pages) { page.SetActive(false); }

        switch (pageToOpen)
        {
            case MenuEnums.MenuStart: { menuStartGO.SetActive(true); break; };
            case MenuEnums.LobbyStart: { lobbyStartMenuGO.SetActive(true); break; };
            case MenuEnums.LobbyCreate: { lobbyCreateGO.SetActive(true); break; };
            case MenuEnums.LobbyQuickJoin: { lobbyQuickJoinGO.SetActive(true); break; };
            case MenuEnums.LobbyPreGame: { lobbyPreGameGO.SetActive(true); break; };
            case MenuEnums.LobbyJoinByCode: { lobbyJoinByCodeGO.SetActive(true); break; };
            default: break;
        }
    }

    public void OpenAlert(string message)
    {
        alertMessageGO.SetActive(true);
        alertMessage.text = message;
    }
    public void CloseAlert() { alertMessageGO.SetActive(false); }
    public void OpenLobbyPreGame(object sender, System.EventArgs e) 
    { 
        OpenPage
            (MenuEnums.LobbyPreGame); }


}
