using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] GameObject lobbyManagerGO;
    LobbyManager lobbyManager;

    private void Start()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);

        try
        {
            lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();
        }
        catch (Exception e) { Debug.Log(e); }

    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);

    }
    public void OpenLobby()
    {
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(true);

        try { lobbyManager.OnOpenMenu(); }
        catch (Exception e) { Debug.Log(e); }
    }
}
