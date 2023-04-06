using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This script creates the managers in the correct order.
*/
public class ManagerLoader : MonoBehaviour
{
    public event EventHandler onManagersCreated;  // lets LobbyPreGame know when lobby is created
    void Start()
    {
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        Debug.Log("Creating Managers");
        gameObject.AddComponent<RelayManager>();
        gameObject.AddComponent<LobbyManager>();
        gameObject.AddComponent<MenuManager>();
        onManagersCreated?.Invoke(this, EventArgs.Empty);
        Debug.Log("Managers created");
    }


}
