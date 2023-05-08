using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyQuickJoin : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] GameObject goButtonGO;
    [SerializeField] GameObject backButtonGO;
    MenuManager menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();
    }

    private bool ValidateInput()
    {
        bool isInputValid = false;
        if (playerNameInput.text.Length <= 1 || playerNameInput.text.Length <= 1)
        {
            menuManager.OpenAlert("Fill out all fields");
        }
        else if (playerNameInput.text.Length > 15)
        { menuManager.OpenAlert("Enter a name less than 15 characters"); }
        else { isInputValid = true; }
        return isInputValid;
    }


    private void OnEnable()
    {
        Button goButton = goButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();

        goButton.onClick.AddListener(async () =>
        {
            if (!ValidateInput()) return;
            bool isLobbyFound = await LobbyManager.Singleton.QuickJoinLobby(playerNameInput.text);
            if (isLobbyFound) menuManager.OpenPage(MenuEnums.LobbyRoom);
            else menuManager.OpenAlert("No open lobbies available!");
        });

        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyMenu); });
    }

}
