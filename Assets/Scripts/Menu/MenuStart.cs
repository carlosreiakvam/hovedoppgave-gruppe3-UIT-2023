using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuStart : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI header;
    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] Button beginButton;
    [SerializeField] Button exitButton;


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

    private void OnEnable()
    {
        header.gameObject.SetActive(false);
    }

}
