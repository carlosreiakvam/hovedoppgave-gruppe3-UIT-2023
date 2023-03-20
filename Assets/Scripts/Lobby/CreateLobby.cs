using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobby : MonoBehaviour
{
    [SerializeField] GameObject playerNameInputGO;
    [SerializeField] GameObject lobbyNameInputGO;
    [SerializeField] GameObject scopeButtonGO;
    [SerializeField] GameObject createButtonGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject lobbyMenu;


    private void Start()
    {
        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();

        Button scopeButton = scopeButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();
        Text playerNameInput = playerNameInputGO.GetComponent<Text>();
        Text lobbyNameInput = playerNameInputGO.GetComponent<Text>();

        backButton.onClick.AddListener(() =>
        {
            menuManager.openPage(lobbyMenu);
        });
    }

}
