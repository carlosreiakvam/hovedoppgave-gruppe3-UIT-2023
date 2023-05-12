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
    [SerializeField] Button beginButton;
    [SerializeField] Button exitButton;
    [SerializeField] GameObject lobbyStartMenu;


    private void Start()
    {
        MenuManager menuManager = GetComponentInParent<MenuManager>();
        LobbyManager lobbyManager = GetComponentInParent<LobbyManager>();

        beginButton.onClick.AddListener(() =>
        {
            menuManager.OpenPage(MenuEnums.LobbyMenu);
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });



    }
}
