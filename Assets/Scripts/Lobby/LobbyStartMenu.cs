using UnityEngine;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] GameObject quickJoinButtonGO;
    [SerializeField] GameObject joinByCodeButtonGO;
    [SerializeField] GameObject CreateButtonGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject quickJoinBackGO;
    [SerializeField] GameObject createLobbyPageGO;
    [SerializeField] GameObject lobbyPreGameGO;

    MenuManager menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();


        Button quickJoinButton = quickJoinButtonGO.GetComponent<Button>();
        Button joinByCodeButton = joinByCodeButtonGO.GetComponent<Button>();
        Button createButton = CreateButtonGO.GetComponent<Button>();
        Button quickJoinBack = quickJoinBackGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();


        // CREATE LOBBY
        createButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyCreate); });

        // LOBBY QUICK JOIN
        quickJoinButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyQuickJoin); });

        // JOIN BY CODE
        joinByCodeButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyJoinByCode); });

        // QUICK JOIN BACK
        quickJoinBack.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.LobbyMenu));

        // BackButton
        backButton.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.StartMenu));
    }

}

