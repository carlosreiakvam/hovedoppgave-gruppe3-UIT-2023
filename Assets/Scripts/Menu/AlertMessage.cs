using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI alertMessage;
    [SerializeField] GameObject alertCloseButtonGO;
    [SerializeField] GameObject menuManagerGO;
    Button alertCloseButton;
    MenuManager menuManager;


    private void OnEnable()
    {
        Debug.Log("alert OnEnable");
        alertCloseButton = alertCloseButtonGO.GetComponent<Button>();
        menuManager = menuManagerGO.GetComponent<MenuManager>();
        alertCloseButton.onClick.AddListener(() => { menuManager.CloseAlert(); });
    }

    private void OnBecameVisible()
    {
        Debug.Log("alert became visible");
    }
}
