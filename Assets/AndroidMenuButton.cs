using UnityEngine;
using UnityEngine.UI;

public class AndroidMenuButton : MonoBehaviour
{
    [SerializeField] Button androidMenuButton;
    [SerializeField] GameStatusSO gameStatusSO;

    void Start()
    {
        if (gameStatusSO.isWindows) gameObject.SetActive(false);
        else gameObject.SetActive(true);

        androidMenuButton.onClick.AddListener(() =>
        {
            InGameMenu.Singleton.ToggleInGameMenu();
        });

    }

}
