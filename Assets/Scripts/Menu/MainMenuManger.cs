using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManger : MonoBehaviour
{

    [SerializeField] GameObject beginButtonGO;

    void Start()
    {
        Button beginButton = beginButtonGO.GetComponent<Button>();
        GameObject parent = this.transform.parent.gameObject;
        MenuManager menuManager = parent.GetComponent<MenuManager>();


        beginButton.onClick.AddListener(() =>
        {
            menuManager.OpenLobby();

        });
    }
}
