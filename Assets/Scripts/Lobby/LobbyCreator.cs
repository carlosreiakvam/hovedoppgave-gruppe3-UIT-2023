using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The LobbyCreator class handles the creation of a lobby in the game.
/// It manages the interaction between player and lobby settings such as name, privacy, and visual feedback.
/// </summary>
public class LobbyCreator : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] Sprite spinnerSprite;
    [SerializeField] TextMeshProUGUI header;
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] TextMeshProUGUI lobbyNameInput;
    [SerializeField] GameObject scopeButtonGO;
    [SerializeField] GameObject createButtonGO;
    [SerializeField] GameObject backButtonGO;
    [SerializeField] GameObject lobbyPreGameGO;
    LobbyManager lobbyManager;
    MenuManager menuManager;
    bool isPrivate = false;
    GameObject spinner;


    private void OnEnable()
    {
        header.transform.gameObject.SetActive(true);
        header.text = "Create Lobby";
    }


    private void Start()
    {

        menuManager = GetComponentInParent<MenuManager>();
        lobbyManager = GetComponentInParent<LobbyManager>();
        spinner = CreateSpinner();


        Button scopeButton = scopeButtonGO.GetComponent<Button>();
        Button createButton = createButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();
        TextMeshProUGUI scopeText = scopeButton.GetComponentInChildren<TextMeshProUGUI>();

        backButton.onClick.AddListener(() =>
        {
            menuManager.OpenPage(MenuEnums.LobbyMenu);
        });

        createButton.onClick.AddListener(async () =>
        {
            if (lobbyNameInput.text.Length <= 1 || playerNameInput.text.Length <= 1)
            { menuManager.OpenAlert("Fill out all fields"); }
            else if (playerNameInput.text.Length > 15)
            { menuManager.OpenAlert("Enter a name less than 15 characters"); }
            else if (lobbyNameInput.text.Length > 15)
            { menuManager.OpenAlert("Enter a lobby name less than 15 characters"); }
            else
            {
                spinner.SetActive(true);
                StartCoroutine(RotateSpinner(spinner));
                await LobbyManager.Singleton.CreateLobby(lobbyNameInput.text, isPrivate, playerNameInput.text);
                spinner.SetActive(false);
            }

        });

        scopeButton.onClick.AddListener(() =>
        {
            isPrivate = !isPrivate;
            scopeText.text = isPrivate ? "Private" : "Public";
        });
    }

    /// <summary>
    /// Coroutine to rotate the spinner object while it is active.
    /// </summary>
    /// <param name="spinner">GameObject that should be rotated.</param>
    /// <returns>Coroutine IEnumerator</returns>
    IEnumerator RotateSpinner(GameObject spinner)
    {
        while (spinner.activeSelf)
        {
            // Rotate the spinner by 1 degree per frame
            spinner.transform.Rotate(Vector3.forward);
            yield return null;
        }
    }


    /// <summary>
    /// Creates a spinner GameObject, sets its sprite and parent, and initially sets it to be invisible.
    /// </summary>
    /// <returns>A GameObject representing the spinner.</returns>
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
