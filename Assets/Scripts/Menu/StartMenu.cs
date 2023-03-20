using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{

    [SerializeField] GameObject beginButtonGO;
    [SerializeField] GameObject lobbyStartMenu;
    [SerializeField] GameObject lobbyManagerGO;

    void Start()
    {
        Button beginButton = beginButtonGO.GetComponent<Button>();
        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();
        LobbyManager lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();


        beginButton.onClick.AddListener(() =>
        {
            menuManager.openPage(lobbyStartMenu);
            lobbyManager.OnOpenLobbyStartMenu();

        });
    }
}
