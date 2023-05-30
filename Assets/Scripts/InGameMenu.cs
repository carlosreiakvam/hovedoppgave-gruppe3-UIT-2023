using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] GameObject inGameMenuPanel;
    [SerializeField] GameObject leaveButtonGO;
    private Button leaveButton;

    private void Start()
    {
        leaveButton = leaveButtonGO.GetComponent<Button>();
        leaveButton.onClick.AddListener(() =>
        {
            GameManager.Singleton.EndGameScene();
        });

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
