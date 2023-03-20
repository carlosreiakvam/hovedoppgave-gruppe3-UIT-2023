using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyStartMenu : MonoBehaviour
{
    [SerializeField] GameObject startGameButtonGO;
    [SerializeField] GameObject CreateButtonGO;
    [SerializeField] GameObject createLobbyPage;

    private void Start()
    {
        Button startGameButton = startGameButtonGO.GetComponent<Button>();
        Button createButton = CreateButtonGO.GetComponent<Button>();

        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();

        startGameButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Game");
            });

        createButton.onClick.AddListener(() =>
        {
            menuManager.openPage(createLobbyPage);
        });

    }
}
