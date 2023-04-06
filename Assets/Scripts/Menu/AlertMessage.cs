using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI alertMessage;
    [SerializeField] GameObject alertCloseButtonGO;
    Button alertCloseButton;
    MenuManager menuManager;

    private void Start()
    {
        menuManager = GetComponentInParent<MenuManager>();

    }

    private void OnEnable()
    {
        Debug.Log("alert OnEnable");
        alertCloseButton = alertCloseButtonGO.GetComponent<Button>();
        alertCloseButton.onClick.AddListener(() => { menuManager.CloseAlert(); });
    }

    private void OnBecameVisible()
    {
        Debug.Log("alert became visible");
    }
}
