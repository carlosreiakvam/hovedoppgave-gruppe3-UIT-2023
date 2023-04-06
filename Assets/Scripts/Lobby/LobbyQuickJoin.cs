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
    LobbyManager lobbyManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();
        lobbyManager = GetComponentInParent<LobbyManager>();
    }


    void OnEnable()
    {
        Button goButton = goButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();

        goButton.onClick.AddListener(() => { lobbyManager.QuickJoinLobby(playerNameInput.text); });
        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyMenu); });
    }

}
