using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Instructions : MonoBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject windowsInstructions;
    [SerializeField] GameObject androidInstructions;
    [SerializeField] Button startGameButton;
    private void OnEnable()
    {
        Debug.Log(gameStatusSO.isWindows);
        windowsInstructions.SetActive(gameStatusSO.isWindows);
        androidInstructions.SetActive(!gameStatusSO.isWindows);

        startGameButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", LoadSceneMode.Single);
        });


    }
}
