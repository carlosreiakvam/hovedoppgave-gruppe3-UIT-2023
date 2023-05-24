using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public event EventHandler OnMenuStartOpened;
    public event EventHandler OnLobbyMenuOpened;
    public event EventHandler OnLobbyCreateOpened;
    public event EventHandler OnLobbyJoinOpened;
    public event EventHandler OnLobbyRoomOpened;
    [SerializeField] GameObject imageControl;
    [SerializeField] private GameStatusSO gameStatusSO;
    [SerializeField] private TextMeshProUGUI alertMessage;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI gametitle;
    [SerializeField] private GameObject chatVisual;
    [SerializeField] List<GameObject> pages;

    public static MenuManager Singleton;

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);

        header.gameObject.SetActive(false);
        chatVisual.SetActive(false);
    }

    private void Start()
    {
        // DISPLAY MESSAGE IF IS ANDROID
        imageControl.SetActive(gameStatusSO.isAndroid);

        // Subscribe to events
        RelayManager.Singleton.OnRelayCreated += OpenLobbyRoom;

        OpenPage(MenuEnums.MenuStart);
    }




    public void OpenPage(MenuEnums pageToOpen)
    {
        header.gameObject.SetActive(pageToOpen != MenuEnums.MenuStart);
        chatVisual.SetActive(pageToOpen == MenuEnums.LobbyRoom);

        foreach (GameObject page in pages)
        {
            page.SetActive(false);
        }


        switch (pageToOpen)
        {
            case MenuEnums.MenuStart:
                pages[0].SetActive(true);
                gametitle.gameObject.SetActive(true);
                header.gameObject.SetActive(false);
                OnMenuStartOpened?.Invoke(this, EventArgs.Empty);
                break;

            case MenuEnums.LobbyMenu:
                header.gameObject.SetActive(true);
                pages[1].SetActive(true);
                gametitle.gameObject.SetActive(false);
                header.text = MenuHeaders.LOBBY_MENU;
                OnLobbyMenuOpened?.Invoke(this, EventArgs.Empty);
                break;
            case MenuEnums.LobbyCreate:
                pages[2].SetActive(true);
                header.text = MenuHeaders.CREATE_LOBBY;
                OnLobbyCreateOpened?.Invoke(this, EventArgs.Empty);
                break;
            case MenuEnums.LobbyQuickJoin:
                pages[3].SetActive(true);
                OnLobbyJoinOpened?.Invoke(this, EventArgs.Empty);
                header.text = MenuHeaders.JOIN_LOBBY;
                break;
            case MenuEnums.LobbyRoom:
                pages[4].SetActive(true);
                OnLobbyRoomOpened?.Invoke(this, EventArgs.Empty);
                break;

            default: break;
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
    {
        OpenPage(MenuEnums.LobbyRoom);
    }

}
