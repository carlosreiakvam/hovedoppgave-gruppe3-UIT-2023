using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] GameObject startGameButtonGO;

    private void Start()
    {
        Button startGameButton = startGameButtonGO.GetComponent<Button>();
        startGameButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Game");
            });
    }

}
