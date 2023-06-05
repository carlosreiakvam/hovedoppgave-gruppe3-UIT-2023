using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI alertMessage;
    [SerializeField] GameObject alertCloseButtonGO;
    Button alertCloseButton;

    private void Start()
    {

    }

    private void OnEnable()
    {
        Debug.Log("alert OnEnable");
        alertCloseButton = alertCloseButtonGO.GetComponent<Button>();
        alertCloseButton.onClick.AddListener(() => { MenuManager.Singleton.CloseAlert(); });
    }

    private void OnBecameVisible()
    {
        Debug.Log("alert became visible");
    }
}
