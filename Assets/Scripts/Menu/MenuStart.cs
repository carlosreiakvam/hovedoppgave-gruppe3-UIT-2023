using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuStart : MonoBehaviour
{

    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] GameObject beginButtonGO;
    [SerializeField] GameObject lobbyStartMenu;


    private void Start()
    {
        MenuManager menuManager = GetComponentInParent<MenuManager>();
        LobbyManager lobbyManager = GetComponentInParent<LobbyManager>();

        Button beginButton = beginButtonGO.GetComponent<Button>();
        beginButton.onClick.AddListener(() =>
        {
            menuManager.OpenPage(MenuEnums.LobbyMenu);
        });


    }
}
