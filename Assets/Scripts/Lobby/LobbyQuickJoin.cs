using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyQuickJoin : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Sprite spinnerSprite;
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] GameObject goButtonGO;
    [SerializeField] GameObject backButtonGO;
    MenuManager menuManager;
    GameObject spinner;

    private void Start()
    {

        menuManager = GetComponentInParent<MenuManager>();
        spinner = CreateSpinner();

        Button goButton = goButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();

        goButton.onClick.AddListener(async () =>
        {
            if (!ValidateInput()) return;
            spinner.SetActive(true);
            StartCoroutine(RotateSpinner(spinner));
            bool isLobbyFound = await LobbyManager.Singleton.QuickJoinLobby(playerNameInput.text);
            spinner.SetActive(false);
            if (isLobbyFound) menuManager.OpenPage(MenuEnums.LobbyRoom);
            else menuManager.OpenAlert("No open lobbies available!");
        });

        backButton.onClick.AddListener(() => { menuManager.OpenPage(MenuEnums.LobbyMenu); });


    }

    private bool ValidateInput()
    {
        bool isInputValid = false;
        if (playerNameInput.text.Length <= 1 || playerNameInput.text.Length <= 1)
        {
            menuManager.OpenAlert("Fill out all fields");
        }
        else if (playerNameInput.text.Length > 15)
        { menuManager.OpenAlert("Enter a name less than 15 characters"); }
        else { isInputValid = true; }
        return isInputValid;
    }


    private void OnEnable()
    {
        header.gameObject.SetActive(true);
        header.text = "Join Lobby";

    }

    IEnumerator RotateSpinner(GameObject spinner)
    {
        while (spinner.activeSelf)
        {
            // Rotate the spinner by 1 degree per frame
            spinner.transform.Rotate(Vector3.forward);
            yield return null;
        }
    }

    public GameObject CreateSpinner()
    {
        // Create a new GameObject for the spinner
        GameObject spinner = new GameObject("Spinner");

        // Add the Image component to the spinner
        Image spinnerImage = spinner.AddComponent<Image>();

        // Assign your spinner sprite to the image component
        spinnerImage.sprite = spinnerSprite;

        // Set the spinner as a child of the canvas
        spinner.transform.SetParent(canvas.transform, false);

        // Initially, set the spinner to be invisible
        spinner.SetActive(false);

        return spinner;
    }

}
