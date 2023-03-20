using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{

    [SerializeField] GameObject beginButtonGO;
    [SerializeField] GameObject lobbyStartMenu;

    void Start()
    {
        Button beginButton = beginButtonGO.GetComponent<Button>();
        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();


        beginButton.onClick.AddListener(() =>
        {
            Debug.Log("tjolla");
            menuManager.openPage(lobbyStartMenu);

        });
    }
}
