using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyJoinByCodeMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] TextMeshProUGUI lobbyCodeInput;
    [SerializeField] GameObject goButtonGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject lobbyManagerGO;
    [SerializeField] GameObject menuManagerGO;


    // Start is called before the first frame update
    void OnEnable()
    {
        Button goButton = goButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();
        LobbyManager lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();
        MenuManager menuManager = menuManagerGO.GetComponent<MenuManager>();

        goButton.onClick.AddListener(() => { lobbyManager.JoinLobbyByCode(lobbyCodeInput.text, playerNameInput.text); });
        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyStart); });

    }

}
