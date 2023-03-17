using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject lobbyMenu;

    private void Start()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        lobbyMenu.SetActive(false);

    }
    public void OpenLobby()
    {
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }
}
