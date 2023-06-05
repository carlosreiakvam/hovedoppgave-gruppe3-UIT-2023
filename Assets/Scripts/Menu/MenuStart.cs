using UnityEngine;
using UnityEngine.UI;

public class MenuStart : MonoBehaviour
{

    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] Button beginButton;
    [SerializeField] Button exitButton;


    private void Start()
    {

        beginButton.onClick.AddListener(() =>
        {
            MenuManager.Singleton.OpenPage(MenuEnums.LobbyMenu);
        });

        exitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });

    }

}
