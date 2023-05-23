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
    [SerializeField] ChatManager chatmanager;
    [SerializeField] private GameStatusSO gameStatusSO;
    [SerializeField] private TextMeshProUGUI alertMessage;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI gametitle;
    [SerializeField] private GameObject chatVisual;
    [SerializeField] List<GameObject> pages;

    private float screenWidth;
    private float screenHeight;
    private float screenRatio;
    private bool isScreenSuperWide;

    private void Awake()
    {
        header.gameObject.SetActive(false);
        chatVisual.SetActive(false);
    }

    private void Start()
    {
        // DISPLAY MESSAGE IF IS ANDROID
        imageControl.SetActive(gameStatusSO.isAndroid);

        Canvas canvas = GetComponent<Canvas>();
        RectTransform rectTransformCanvas = canvas.GetComponent<RectTransform>();
        screenWidth = rectTransformCanvas.sizeDelta.x;
        screenHeight = rectTransformCanvas.sizeDelta.y;
        screenRatio = screenWidth / screenHeight;
        isScreenSuperWide = (screenRatio > 2f);

        Debug.LogWarning("Screen width: " + screenWidth);
        Debug.LogWarning("Screen height: " + screenHeight);

        Debug.LogWarning("Screenratio: " + screenRatio);
        Debug.LogWarning("superwide: " + isScreenSuperWide);

        SetMenuSizes();
        SetChatSize();
        SetHeader(1.5f, 4.3f, 3.5f, header);
        SetHeader(1.5f, 5.4f, 3, gametitle);

        // Lobby room is opened when a relay is created.
        RelayManager.Singleton.OnRelayCreated += OpenLobbyRoom;

        OpenPage(MenuEnums.MenuStart);
    }

    private void SetHeader(float widtRatio, float heightRatio, float yOffsetRatio, TextMeshProUGUI header)
    {
        float width = screenWidth / widtRatio;
        float height = screenHeight / heightRatio;

        RectTransform headerTransform = header.GetComponent<RectTransform>();
        headerTransform.sizeDelta = new Vector2(width, height);
        headerTransform.anchoredPosition = new Vector2(0, -(screenHeight / yOffsetRatio));
    }

    private void SetChatSize()
    {
        float chatWidth, chatHeight;
        chatWidth = 500;
        if (isScreenSuperWide)
        {
            chatHeight = 300;
            chatmanager.SetMaxMessages(6);
        }
        else
        {
            chatHeight = 500;
            chatmanager.SetMaxMessages(11);
        }

        RectTransform chatTransform = chatVisual.GetComponent<RectTransform>();
        chatTransform.sizeDelta = new Vector2(chatWidth, chatHeight);
        float offsetX = (chatWidth / 2f);
        float offsetY = -(chatHeight / 2f);
        chatTransform.anchoredPosition = new Vector2(offsetX, offsetY);
    }

    private void SetMenuSizes()
    {
        int margin = 10;
        int maxVertElements = 4;
        float nButtonsToFitInPanel = 1.2f;

        float panelHeight = screenHeight / 2.5f;
        float panelWidth = screenWidth / 3;

        float buttonHeight = (panelHeight - (margin * maxVertElements)) / maxVertElements;
        float buttonWidth = panelWidth / nButtonsToFitInPanel;

        float buttonPanelHeight = buttonHeight;
        float playersElementPanelHeight = panelHeight - buttonPanelHeight;

        float playerReadyWidth = 150f;
        float playerNameWidth = panelWidth - playerReadyWidth;

        float playerElementHeight = playersElementPanelHeight / maxVertElements;

        foreach (GameObject page in pages)
        {
            RectTransform rectTransform = page.GetComponent<RectTransform>();

            float offsetY = -(panelHeight / 3f);
            rectTransform.anchoredPosition = new Vector2(0, offsetY);
            rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);

            if (page.name != MenuEnums.LobbyRoom.ToString())
            {
                SetChildrenSizes(page, buttonWidth, buttonHeight);
            }
            else
            {
                // lobbyRoom
                for (int i = 0; i < page.transform.childCount; i++)
                {
                    // Each lobby room panel

                    GameObject childPanel = page.transform.GetChild(i).gameObject;
                    RectTransform childRectTransform = childPanel.GetComponent<RectTransform>();

                    switch (childPanel.name)
                    {
                        case "DoubleButtons":
                            {
                                childRectTransform.sizeDelta = new Vector2(panelWidth, buttonPanelHeight);
                                float buttonPanelOffset = -(buttonHeight / 2);
                                childRectTransform.anchoredPosition = new Vector2(0, buttonPanelOffset);

                                SetChildrenSizes(childPanel, panelWidth / 2, buttonHeight);
                                break;
                            }
                        case "SingleButton":
                            {
                                childRectTransform.sizeDelta = new Vector2(panelWidth, buttonPanelHeight);
                                float buttonPanelOffset = +(buttonHeight / 2);
                                childRectTransform.anchoredPosition = new Vector2(0, buttonPanelOffset);

                                SetChildrenSizes(childPanel, panelWidth, buttonHeight);
                                break;
                            }

                        case "Players":
                            {
                                childRectTransform.sizeDelta = new Vector2(panelWidth, playersElementPanelHeight);
                                float playerPanelOffset = -(playersElementPanelHeight / 2);
                                childRectTransform.anchoredPosition = new Vector2(0, playerPanelOffset);

                                SetChildrenSizes(childPanel, playerNameWidth, playerElementHeight);
                                break;
                            }
                        case "ReadyStates":
                            {
                                childRectTransform.sizeDelta = new Vector2(panelWidth, playersElementPanelHeight);
                                float readyPanelOffset = -(playersElementPanelHeight / 2);
                                childRectTransform.anchoredPosition = new Vector2(0, readyPanelOffset);

                                SetChildrenSizes(childPanel, playerReadyWidth, playerElementHeight);
                                break;
                            }
                        default: { break; }
                    }
                }
            }
        }
    }

    private void SetChildrenSizes(GameObject page, float width, float height)
    {
        for (int i = 0; i < page.transform.childCount; i++)
        {
            Transform child = page.transform.GetChild(i);
            {
                RectTransform childRectTransform = child.GetComponent<RectTransform>();
                childRectTransform.sizeDelta = new Vector2(width, height);
            }

        }
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
