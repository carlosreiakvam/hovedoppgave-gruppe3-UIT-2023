using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject largeMessage;
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] TextMeshProUGUI infoText;

    public static GameManager Singleton;
    public ulong networkedPlayerIdHasRing { get; set; }

    private void Awake()
    {
        largeMessage.SetActive(false);
        if (Singleton == null) { Singleton = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    private void Start()
    {
        LocalPlayerManager.Singleton.RegisterPlayerInScriptableObject();
    }



    public void StartGameManagerServer()
    {
        Debug.LogWarning("GAMEMANAGER STARTED");

        if (SpawnManager.Singleton.SpawnAll()) { Debug.LogWarning("SPAWNING SUCCESSFULL"); }
        else { Debug.LogError("SPAWNING FAILED"); }
    }


    public void EndGameScene()
    {
        try
        {
            NetworkManager.Singleton.Shutdown();
            DestroyObjectsTaggedWithDontDestroy();
            SceneManager.LoadScene("Menus");
        }
        catch (Exception e)
        {
            Debug.LogError(e); return;
        }
    }

    // Destroy objects tagged with "DontDestroy" when going back to the menu scene
    private void DestroyObjectsTaggedWithDontDestroy()
    {
        GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in objs)
        {
            if (obj.transform.parent == null && obj.transform != transform && obj.scene.name != null)
            {
                if (obj.CompareTag("DontDestroy")) Destroy(obj);
                Debug.LogWarning("Destroyed: " + obj.name);
            }
        }
    }

    public void OnPlayerPickedUpRing(string name)
    {
        ChatManager.Instance.SendMsg(name + " collected a ring!", "Wizard");
        //infoText.text = name + " collected a ring!";

    }


    public void VisualizeOnGameWon(string playerName)
    {
        gameStatusSO.gameIsOver = true;
        infoText.text = playerName + " won the game!";
        inGameMenu.SetActive(true);
        largeMessage.SetActive(true);
    }

    public void DeactivateAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
    }

}

