using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneShortcut : MonoBehaviour
{
    [SerializeField] GameStatusSO gamestatus;
    [SerializeField] Button gameSceneButtonGO;

    void Start()
    {

        gameSceneButtonGO.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
            gamestatus.isShortcutUsed = true;
        });
    }
}
