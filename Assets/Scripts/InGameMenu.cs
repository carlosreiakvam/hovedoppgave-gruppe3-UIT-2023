using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject leaveButtonGO;
    [SerializeField] GameObject inGameMenuPanel;

    private Button leaveButton;
    public static InGameMenu Singleton;

    private void Awake()
    {
        if (Singleton == null) { Singleton = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void Start()
    {
        inGameMenuPanel.SetActive(false);
        leaveButton = leaveButtonGO.GetComponent<Button>();
        leaveButton.onClick.AddListener(() =>
        {
            GameManager.Singleton.EndGameScene();
        });

    }

    public void ShowInGameMenu(bool show)
    {
        inGameMenuPanel.SetActive(show);
    }

    public void ToggleInGameMenu()
    {
        inGameMenuPanel.SetActive(!inGameMenuPanel.activeSelf);

    }




    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameStatusSO.gameIsOver)
            {
                inGameMenuPanel.SetActive(!inGameMenuPanel.activeSelf);
            }
        }
    }
}
