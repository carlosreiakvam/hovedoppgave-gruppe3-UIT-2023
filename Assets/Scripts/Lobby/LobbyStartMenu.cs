using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] private Button quickJoinButtonGO;
    [SerializeField] private Button CreateButtonGO;
    [SerializeField] private Button backButtonGO;

    MenuManager menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();

        // CREATE LOBBY
        CreateButtonGO.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyCreate); });

        // LOBBY QUICK JOIN
        quickJoinButtonGO.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyQuickJoin); });

        // BackButton
        backButtonGO.onClick.AddListener(() => menuManager.OpenPage(MenuEnums.MenuStart));
    }

}

