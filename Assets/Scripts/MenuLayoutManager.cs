using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Manages the layout of various menus in the game, including setting sizes and positions for UI elements.
/// </summary>
public class MenuLayoutManager : MonoBehaviour
{
    private const float SCREEN_SUPER_WIDE_RATIO = 2f;
    private const float CHAT_WIDTH = 500f;
    private const float CHAT_HEIGHT_SUPER_WIDE = 300f;
    private const float CHAT_HEIGHT_REGULAR = 500f;
    private const int CHAT_MAX_MESSAGES_SUPER_WIDE = 6;
    private const int CHAT_MAX_MESSAGES_REGULAR = 11;


    private float screenWidth, screenHeight, screenRatio, panelWidth, panelHeight;
    private bool isScreenSuperWide;

    [SerializeField] Canvas menuCanvas;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private TextMeshProUGUI gametitle;
    [SerializeField] private GameObject chatVisual;
    [SerializeField] List<GameObject> pages;


    private void Start()
    {
        InitializeMenus();
    }

    /// <summary>
    /// Initializes the menus by setting screen size variables and UI element sizes.
    /// </summary>
    private void InitializeMenus()
    {
        SetScreenSizeVariables();
        SetMenuSizes();
        SetChatSize();
        SetHeaderSizeAndPosition(1.5f, 4.3f, 3.5f, header);
        SetHeaderSizeAndPosition(1.5f, 5.4f, 3, gametitle);
    }

    /// <summary>
    /// Sets screen size variables such as width, height, and ratio.
    /// </summary>
    private void SetScreenSizeVariables()
    {
        RectTransform rectTransformCanvas = menuCanvas.GetComponent<RectTransform>();
        screenWidth = rectTransformCanvas.sizeDelta.x;
        screenHeight = rectTransformCanvas.sizeDelta.y;
        screenRatio = screenWidth / screenHeight;
        isScreenSuperWide = (screenRatio > SCREEN_SUPER_WIDE_RATIO);
        panelHeight = screenHeight / 2.5f;
        panelWidth = screenWidth / 3;
    }


    /// <summary>
    /// Sets the sizes of the menu pages.
    /// </summary>
    private void SetMenuSizes()
    {
        const int MARGIN = 10;
        const int MAX_VERT_ELEMENTS = 4;
        const float N_BUTTONS_IN_PANEL = 1.2f;

        float buttonHeight = (panelHeight - (MARGIN * MAX_VERT_ELEMENTS)) / MAX_VERT_ELEMENTS;
        float buttonWidth = panelWidth / N_BUTTONS_IN_PANEL;
        float buttonPanelHeight = buttonHeight;


        foreach (GameObject page in pages)
        {
            SetChildrenElementSizes(page, buttonWidth, buttonHeight, buttonPanelHeight, MAX_VERT_ELEMENTS);
        }
    }


    /// <summary>
    /// Sets the sizes of the children UI elements in a page.
    /// </summary>
    /// <param name="page">The parent GameObject containing the children elements.</param>
    /// <param name="buttonWidth">The desired width of the buttons.</param>
    /// <param name="buttonHeight">The desired height of the buttons.</param>
    /// <param name="buttonPanelHeight">The desired height of the button panel.</param>
    /// <param name="MAX_VERT_ELEMENTS">The maximum number of vertical elements in the layout.</param>
    private void SetChildrenElementSizes(GameObject page, float buttonWidth, float buttonHeight, float buttonPanelHeight, int MAX_VERT_ELEMENTS)
    {
        RectTransform rectTransform = page.GetComponent<RectTransform>();

        float offsetY = -(panelHeight / 3f);
        rectTransform.anchoredPosition = new Vector2(0, offsetY);
        rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);

        if (page.name != MenuEnums.LobbyRoom.ToString())
        {
            SetGeneralChildrenElementSizes(page, buttonWidth, buttonHeight);
        }
        else
        {
            SetLobbyRoomChildrenElementSizes(page, buttonHeight, buttonPanelHeight, MAX_VERT_ELEMENTS);
        }
    }


    /// <summary>
    /// Sets the sizes of elements within the lobby room for a given page, using specified button height, button panel height, and the maximum vertical elements.
    /// </summary>
    /// <param name="page">The GameObject page to set sizes for.</param>
    /// <param name="buttonHeight">The desired height of the button.</param>
    /// <param name="buttonPanelHeight">The desired height of the button panel.</param>
    /// <param name="maxVertElements">The maximum number of vertical elements in the layout.</param>
    private void SetLobbyRoomChildrenElementSizes(GameObject page, float buttonHeight, float buttonPanelHeight, int maxVertElements)
    {
        const float PLAYER_READY_WIDTH = 150f;
        float playerNameWidth = panelWidth - PLAYER_READY_WIDTH;

        float playersElementPanelHeight = panelHeight - buttonPanelHeight;
        float playerElementHeight = playersElementPanelHeight / maxVertElements;
        RectTransform childRectTransform;

        for (int i = 0; i < page.transform.childCount; i++)
        {
            // for each lobby room panel
            GameObject childPanel = page.transform.GetChild(i).gameObject;
            childRectTransform = childPanel.GetComponent<RectTransform>();

            switch (childPanel.name)
            {
                case "DoubleButtons":
                    {
                        childRectTransform.sizeDelta = new Vector2(panelWidth, buttonPanelHeight);
                        float buttonPanelOffset = -(buttonHeight / 2);
                        childRectTransform.anchoredPosition = new Vector2(0, buttonPanelOffset);

                        SetGeneralChildrenElementSizes(childPanel, panelWidth / 2, buttonHeight);
                        break;
                    }
                case "SingleButton":
                    {
                        childRectTransform.sizeDelta = new Vector2(panelWidth, buttonPanelHeight);
                        float buttonPanelOffset = +(buttonHeight / 2);
                        childRectTransform.anchoredPosition = new Vector2(0, buttonPanelOffset);

                        SetGeneralChildrenElementSizes(childPanel, panelWidth, buttonHeight);
                        break;
                    }

                case "Players":
                    {
                        childRectTransform.sizeDelta = new Vector2(panelWidth, playersElementPanelHeight);
                        float playerPanelOffset = -(playersElementPanelHeight / 2);
                        childRectTransform.anchoredPosition = new Vector2(0, playerPanelOffset);

                        SetGeneralChildrenElementSizes(childPanel, playerNameWidth, playerElementHeight);
                        break;
                    }
                case "ReadyStates":
                    {
                        childRectTransform.sizeDelta = new Vector2(panelWidth, playersElementPanelHeight);
                        float readyPanelOffset = -(playersElementPanelHeight / 2);
                        childRectTransform.anchoredPosition = new Vector2(0, readyPanelOffset);

                        SetGeneralChildrenElementSizes(childPanel, PLAYER_READY_WIDTH, playerElementHeight);
                        break;
                    }
                default: { break; }
            }
        }

    }


    /// <summary>
    /// Sets the sizes of the children UI elements in a page.
    /// </summary>
    /// <param name="page">The parent GameObject containing the children elements.</param>
    /// <param name="width">The width to set for the children elements.</param>
    /// <param name="height">The height to set for the children elements.</param>
    private void SetGeneralChildrenElementSizes(GameObject page, float width, float height)
    {
        RectTransform childRectTransform;
        for (int i = 0; i < page.transform.childCount; i++)
        {
            Transform child = page.transform.GetChild(i);
            {
                childRectTransform = child.GetComponent<RectTransform>();
                childRectTransform.sizeDelta = new Vector2(width, height);
            }

        }
    }


    /// <summary>
    /// Sets the size of the chat box UI element.
    /// </summary>
    private void SetChatSize()
    {
        float chatWidth, chatHeight;
        chatWidth = CHAT_WIDTH;
        if (isScreenSuperWide)
        {
            chatHeight = CHAT_HEIGHT_SUPER_WIDE;
            ChatManager.Instance.SetMaxMessages(CHAT_MAX_MESSAGES_SUPER_WIDE);
        }
        else
        {
            chatHeight = CHAT_HEIGHT_REGULAR;
            ChatManager.Instance.SetMaxMessages(CHAT_MAX_MESSAGES_REGULAR);
        }

        RectTransform chatTransform = chatVisual.GetComponent<RectTransform>();
        chatTransform.sizeDelta = new Vector2(chatWidth, chatHeight);
        float offsetX = (chatWidth / 2f);
        float offsetY = -(chatHeight / 2f);
        chatTransform.anchoredPosition = new Vector2(offsetX, offsetY);
    }


    /// <summary>
    /// Sets the size and position of a header UI element.
    /// </summary>
    /// <param name="widthRatio">The ratio to divide the screen width by to set the header width.</param>
    /// <param name="heightRatio">The ratio to divide the screen height by to set the header height.</param>
    /// <param name="yOffsetRatio">The ratio to divide the screen height by to set the header's y-offset.</param>
    /// <param name="header">The TextMeshProUGUI object to resize and reposition.</param>
    private void SetHeaderSizeAndPosition(float widtRatio, float heightRatio, float yOffsetRatio, TextMeshProUGUI header)
    {
        float width = screenWidth / widtRatio;
        float height = screenHeight / heightRatio;

        RectTransform headerTransform = header.GetComponent<RectTransform>();
        headerTransform.sizeDelta = new Vector2(width, height);
        headerTransform.anchoredPosition = new Vector2(0, -(screenHeight / yOffsetRatio));
    }
}
