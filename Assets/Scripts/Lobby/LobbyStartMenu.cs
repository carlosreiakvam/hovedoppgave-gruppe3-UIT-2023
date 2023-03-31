using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] GameObject quickJoinButtonGO;
    [SerializeField] GameObject joinByCodeButtonGO;
    [SerializeField] GameObject CreateButtonGO;
    [SerializeField] GameObject menuManagerGO;
    [SerializeField] GameObject lobbyManagerGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject quickJoinBackGO;
    [SerializeField] GameObject createLobbyPageGO;
    [SerializeField] GameObject lobbyPreGameGO;

    MenuManager menuManager;
    LobbyManager lobbyManager;

    private void Start()
    {
        Button quickJoinButton = quickJoinButtonGO.GetComponent<Button>();
        Button joinByCodeButton = joinByCodeButtonGO.GetComponent<Button>();
        Button createButton = CreateButtonGO.GetComponent<Button>();
        Button quickJoinBack = quickJoinBackGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();
        lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();

        GameObject parent = this.transform.parent.gameObject;
        menuManager = menuManagerGO.GetComponent<MenuManager>();


        // CREATE LOBBY
        createButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyCreate); });

        // LOBBY QUICK JOIN
        quickJoinButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyQuickJoin); });

        // JOIN BY CODE
        joinByCodeButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyJoinByCode); });

        // QUICK JOIN BACK
        quickJoinBack.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.LobbyStart));

        // BackButton
        backButton.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.MenuStart));
    }

}

