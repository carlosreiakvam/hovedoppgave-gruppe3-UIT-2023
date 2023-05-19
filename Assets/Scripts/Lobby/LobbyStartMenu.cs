using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private Button quickJoinButtonGO;
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

        // QUICK JOIN BACK
        quickJoinBackGO.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.LobbyMenu));

        // BackButton
        backButtonGO.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.MenuStart));
    }

    void OnEnable()
    {
        header.gameObject.SetActive(true);
        header.text = "Lobby Menu";
    }


}

