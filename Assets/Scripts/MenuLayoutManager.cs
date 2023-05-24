using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuLayoutManager : MonoBehaviour
{
    private float screenWidth, screenHeight, screenRatio;
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

    private void InitializeMenus()
    {
        SetScreenSizeVariables();
        SetMenuSizes();
        SetChatSize();
        SetHeaderSizeAndPosition(1.5f, 4.3f, 3.5f, header);
        SetHeaderSizeAndPosition(1.5f, 5.4f, 3, gametitle);
    }
    private void SetScreenSizeVariables()
    {
        RectTransform rectTransformCanvas = menuCanvas.GetComponent<RectTransform>();
        screenWidth = rectTransformCanvas.sizeDelta.x;
        screenHeight = rectTransformCanvas.sizeDelta.y;
        screenRatio = screenWidth / screenHeight;
        isScreenSuperWide = (screenRatio > 2f);
        /*        Debug.LogWarning("Screen width: " + screenWidth);
                Debug.LogWarning("Screen height: " + screenHeight);

                Debug.LogWarning("Screenratio: " + screenRatio);
                Debug.LogWarning("superwide: " + isScreenSuperWide);
        */
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


    private void SetChatSize()
    {
        float chatWidth, chatHeight;
        chatWidth = 500;
        if (isScreenSuperWide)
        {
            chatHeight = 300;
            ChatManager.Instance.SetMaxMessages(6);
        }
        else
        {
            chatHeight = 500;
            ChatManager.Instance.SetMaxMessages(11);
        }

        RectTransform chatTransform = chatVisual.GetComponent<RectTransform>();
        chatTransform.sizeDelta = new Vector2(chatWidth, chatHeight);
        float offsetX = (chatWidth / 2f);
        float offsetY = -(chatHeight / 2f);
        chatTransform.anchoredPosition = new Vector2(offsetX, offsetY);
    }


    private void SetHeaderSizeAndPosition(float widtRatio, float heightRatio, float yOffsetRatio, TextMeshProUGUI header)
    {
        float width = screenWidth / widtRatio;
        float height = screenHeight / heightRatio;

        RectTransform headerTransform = header.GetComponent<RectTransform>();
        headerTransform.sizeDelta = new Vector2(width, height);
        headerTransform.anchoredPosition = new Vector2(0, -(screenHeight / yOffsetRatio));
    }
}
