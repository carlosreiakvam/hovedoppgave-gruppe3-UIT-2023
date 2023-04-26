using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject leaveButtonGO;
    private Button leaveButton;

    private void Start()
    {
        leaveButton = leaveButtonGO.GetComponent<Button>();
        leaveButton.onClick.AddListener(() =>
        {
            SpawnManager.Singleton.DisconnectClientRpc(NetworkManager.Singleton.LocalClientId);
            SceneManager.LoadScene("Menus");
        });

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Toggle inGameMenu's active state
            Debug.Log("MENU?");
            inGameMenu.SetActive(!inGameMenu.activeSelf);
        }
    }
}
