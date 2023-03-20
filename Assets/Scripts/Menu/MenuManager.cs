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
    [SerializeField] GameObject createMenu;
    [SerializeField] GameObject lobbyManagerGO;
    LobbyManager lobbyManager;
    List<GameObject> pages;

    private void Start()
    {
        pages = new List<GameObject> { mainMenu, lobbyMenu, createMenu };
        openPage(mainMenu);

        try
        {
            lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();
        }
        catch (Exception e) { Debug.Log(e); }

    }

    public void openPage(GameObject pageToOpen)
    {
        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }
        pageToOpen.SetActive(true);
    }


    public void OpenLobbyStartMenu()
    {
        try { lobbyManager.OnOpenLobbyStartMenu(); }
        catch (Exception e) { Debug.Log(e); }
    }
}
