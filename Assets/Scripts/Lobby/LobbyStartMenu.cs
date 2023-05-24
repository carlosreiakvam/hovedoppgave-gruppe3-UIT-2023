using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    // GameObjects
    [SerializeField] private Button quickJoinButtonGO;
    [SerializeField] private Button CreateButtonGO;
    [SerializeField] private Button backButtonGO;


    private void Start()
    {

        // CREATE LOBBY
        CreateButtonGO.onClick.AddListener(() => { MenuManager.Singleton.OpenPage(MenuEnums.LobbyCreate); });

        // LOBBY QUICK JOIN
        quickJoinButtonGO.onClick.AddListener(() => { MenuManager.Singleton.OpenPage(MenuEnums.LobbyQuickJoin); });

        // BackButton
        backButtonGO.onClick.AddListener(() => MenuManager.Singleton.OpenPage(MenuEnums.MenuStart));
    }

}

