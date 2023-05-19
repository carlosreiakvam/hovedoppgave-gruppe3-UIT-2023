using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI alertMessage;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private GameObject chatVisual;
    [SerializeField] List<GameObject> pages;

    private void Start()
    {
        RelayManager.Singleton.OnRelayCreated += OpenLobbyRoom;
        InitializeMenu();
        OpenPage(MenuEnums.MenuStart);
    }

    private void InitializeMenu()
    {
        // Hide objects
        header.transform.gameObject.SetActive(false);
        chatVisual.SetActive(false);
    }



    public void OpenPage(MenuEnums pageToOpen)
    {
        if (pageToOpen == MenuEnums.LobbyRoom)
            chatVisual.SetActive(true);


        foreach (GameObject page in pages)
        {
            page.SetActive(false);
            if (page.name == pageToOpen.ToString()) page.SetActive(true);
        }
    }

    public void DestroyMenus()
    { Destroy(gameObject); }


    public void OpenAlert(string message)
    {
        alertMessage.transform.parent.gameObject.SetActive(true);
        alertMessage.text = message;

    }
    public void CloseAlert()
    {
        alertMessage.transform.parent.gameObject.SetActive(false);
    }
    public void OpenLobbyRoom(object sender, System.EventArgs e)
    { OpenPage(MenuEnums.LobbyRoom); }

}
