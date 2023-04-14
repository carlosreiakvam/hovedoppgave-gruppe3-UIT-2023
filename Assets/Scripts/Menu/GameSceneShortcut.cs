using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneShortcut : MonoBehaviour
{
    [SerializeField] Button gameSceneButtonGO;
    public bool isShortcutUsed = false;

    // Start is called before the first frame update
    void Start()
    {

        DontDestroyOnLoad(gameObject);
        gameSceneButtonGO.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
            isShortcutUsed = true;

        });

    }

}
