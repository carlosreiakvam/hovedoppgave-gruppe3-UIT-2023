using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    List<GameObject> pages;
    Transform parentTransform;
    GameObject alertMessageContainerGO;
    TextMeshProUGUI alertMessage;

    private void Start()
    {
        try
        {
            alertMessageContainerGO = gameObject.transform.Find("AlertMessage").gameObject;
            alertMessage = alertMessageContainerGO.GetComponentInChildren<TextMeshProUGUI>();
        } catch (Exception e)
        {
            Debug.LogWarning("Alerts not working. Check MenuManager Start method");
            Debug.Log(e);
        }


        RelayManager.Instance.OnRelayCreated += OpenLobbyRoom;

        parentTransform = gameObject.transform;
        pages = new List<GameObject>();

        // Get menupage gameobjects
        for (int i = 0; i < parentTransform.childCount; i++)
        {
            GameObject child = parentTransform.GetChild(i).gameObject;

            if (child.name == "BG") child.SetActive(true); // Exclude background gameobject
            else pages.Add(child);
        }

        OpenPage(MenuEnums.StartMenu);

    }



    public void OpenPage(MenuEnums pageToOpen)
    {
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
        alertMessageContainerGO.SetActive(true);
        alertMessage.text = message;

    }
    public void CloseAlert()
    {
        alertMessageContainerGO.SetActive(false);
    }
    public void OpenLobbyRoom(object sender, System.EventArgs e)
    { OpenPage(MenuEnums.LobbyRoom); }

}
