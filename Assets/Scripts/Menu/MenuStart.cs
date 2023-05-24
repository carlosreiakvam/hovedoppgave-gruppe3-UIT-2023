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

    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] Button beginButton;
    [SerializeField] Button exitButton;


    private void Start()
    {

        beginButton.onClick.AddListener(() =>
        {
            MenuManager.Singleton.OpenPage(MenuEnums.LobbyMenu);
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

    }

}
