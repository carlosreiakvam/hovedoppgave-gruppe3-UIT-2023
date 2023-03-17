using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject beginButtonGO;
    [SerializeField] GameObject startGameButtonGO;
    [SerializeField] GameObject startMenu;
    [SerializeField] GameObject lobbyMenu;

    private void Start()
    {
        lobbyMenu.SetActive(false);
        Button beginButton = beginButtonGO.GetComponent<Button>();
        Button startGameButton = startGameButtonGO.GetComponent<Button>();


        beginButton.onClick.AddListener(() =>
        {
            startMenu.SetActive(false);
            lobbyMenu.SetActive(true);
        });

        startGameButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Game");
        });

    }
}
