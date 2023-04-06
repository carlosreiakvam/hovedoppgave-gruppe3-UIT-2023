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
    MenuManager menuManager;
    LobbyManager lobbyManager;


    // Start is called before the first frame update
    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();
        lobbyManager = GetComponentInParent<LobbyManager>();

    }
    void OnEnable()
    {
        Button goButton = goButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();

        goButton.onClick.AddListener(() => { lobbyManager.JoinLobbyByCode(lobbyCodeInput.text, playerNameInput.text); });
        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyMenu); });

    }

}
