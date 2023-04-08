using UnityEngine;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] private Button quickJoinButtonGO;
    [SerializeField] private Button joinByCodeButtonGO;
    [SerializeField] private Button CreateButtonGO;
    [SerializeField] private Button backButtonGO;
    [SerializeField] private Button quickJoinBackGO;
    [SerializeField] private Button createLobbyPageGO;
    [SerializeField] private Button lobbyPreGameGO;

    MenuManager menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();

        // CREATE LOBBY
        CreateButtonGO.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyCreate); });

        // LOBBY QUICK JOIN
        quickJoinButtonGO.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyQuickJoin); });

        // JOIN BY CODE
        joinByCodeButtonGO.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyJoinByCode); });

        // QUICK JOIN BACK
        quickJoinBackGO.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.LobbyMenu));

        // BackButton
        backButtonGO.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.StartMenu));
    }

}

