using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobby : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] TextMeshProUGUI lobbyNameInput;
    [SerializeField] GameObject scopeButtonGO;
    [SerializeField] GameObject createButtonGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject lobbyMenu;
    [SerializeField] GameObject lobbyManagerGO;


    private void Start()
    {
        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();
        LobbyManager lobbyManager = lobbyManagerGO.GetComponent<LobbyManager>();

        Button scopeButton = scopeButtonGO.GetComponent<Button>();
        Button createButton = createButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();
        TextMeshProUGUI scopeText = scopeButton.GetComponentInChildren<TextMeshProUGUI>();
        bool isPrivate = true;


        backButton.onClick.AddListener(() =>
        {
            menuManager.openPage(lobbyMenu);
        });

        createButton.onClick.AddListener(() =>
        {
            lobbyManager.CreateLobby(lobbyNameInput.text, isPrivate, playerNameInput.text);
        });

        scopeButton.onClick.AddListener(() =>
        {
            if (isPrivate)
            {
                scopeText.text = "Public";
                isPrivate = false;
            }
            else
            {
                isPrivate = true;
                scopeText.text = "Private";
            }

        });
    }

}
