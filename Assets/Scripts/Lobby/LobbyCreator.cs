using System.Collections;
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
    [SerializeField] TextMeshProUGUI playerNameInput;
    [SerializeField] TextMeshProUGUI lobbyNameInput;
    [SerializeField] GameObject createButtonGO;
    [SerializeField] GameObject backButtonGO;
    bool isPrivate = false;
    GameObject spinner;


    private void Start()
    {

        GameObject verticalLayout = transform.GetChild(0).gameObject;

        spinner = CreateSpinner();


        Button createButton = createButtonGO.GetComponent<Button>();
        Button backButton = backButtonGO.GetComponent<Button>();

        backButton.onClick.AddListener(() =>
        {
            MenuManager.Singleton.OpenPage(MenuEnums.LobbyMenu);
        });

        createButton.onClick.AddListener(async () =>
        {
            if (lobbyNameInput.text.Length <= 1 || playerNameInput.text.Length <= 1)
            { MenuManager.Singleton.OpenAlert("Fill out all fields"); }
            else if (playerNameInput.text.Length > 15)
            { MenuManager.Singleton.OpenAlert("Enter a name less than 15 characters"); }
            else if (lobbyNameInput.text.Length > 15)
            { MenuManager.Singleton.OpenAlert("Enter a lobby name less than 15 characters"); }
            else
            {
                spinner.SetActive(true);
                StartCoroutine(RotateSpinner(spinner));
                await LobbyManager.Singleton.CreateLobby(lobbyNameInput.text, isPrivate, playerNameInput.text);
                spinner.SetActive(false);
            }

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
        GameObject spinner = new GameObject("Spinner");
        Image spinnerImage = spinner.AddComponent<Image>();
        spinnerImage.sprite = spinnerSprite;
        spinner.transform.SetParent(canvas.transform, false);
        spinner.SetActive(false);

        return spinner;
    }
}
