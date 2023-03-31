using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyQuickJoinMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameInput;
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

        goButton.onClick.AddListener(() => { lobbyManager.QuickJoinLobby(playerNameInput.text); });
        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyStart); });

    }

}
