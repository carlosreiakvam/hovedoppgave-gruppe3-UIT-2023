using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuStart : MonoBehaviour
{

    [SerializeField] GameObject beginButtonGO;
    [SerializeField] GameObject lobbyStartMenu;


    private void Start()
    {
        MenuManager menuManager = GetComponentInParent<MenuManager>();
        LobbyManager lobbyManager = GetComponentInParent<LobbyManager>();

        Button beginButton = beginButtonGO.GetComponent<Button>();
        beginButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyMenu); });
    }
}
