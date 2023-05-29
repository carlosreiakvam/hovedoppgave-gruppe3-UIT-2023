using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The main manager for controlling game state.
/// </summary>
public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject inGameMenu;
    [SerializeField] GameObject largeMessage;
    [SerializeField] GameStatusSO gameStatusSO;
    [SerializeField] TextMeshProUGUI infoText;

    // Singleton instance of GameManager
    public static GameManager Singleton;

    // Represents the network player ID who currently has the ring
    public ulong networkedPlayerIdHasRing { get; set; }

    private void Awake()
    {
        largeMessage.SetActive(false);
        if (Singleton == null) { Singleton = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        // Check if the server (host) disconnected
        if (clientId == 0)
        {
            EndGameScene();
        }
        else
        {
            ChatManager.Instance.SendMsg("A player has disconnected from the game.", "Wizard");
        }


    }

    private void Start()
    {
        LocalPlayerManager.Singleton.RegisterPlayerInScriptableObject();
    }

    private void LateUpdate()
    {
    }

    /// <summary>
    /// Begins the game manager server.
    /// </summary>
    public void StartGameManagerServer()
    {
        Debug.LogWarning("GAMEMANAGER STARTED");

        if (SpawnManager.Singleton.SpawnAll()) { Debug.LogWarning("SPAWNING SUCCESSFULL"); }
        else { Debug.LogError("SPAWNING FAILED"); }
    }

    /// <summary>
    /// Handles ending the game scene and returning to the main menu.
    /// </summary>
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

    /// <summary>
    /// Handles the event when a player dies.
    /// </summary>
    public void OnPlayerDeath()
    {

        if (LocalPlayerManager.Singleton.localPlayer.isDead) return;
        ChatManager.Instance.SendMsg(LocalPlayerManager.Singleton.localPlayer.name + " died a tragic death!", "Wizard");
        infoText.text = " You died!";
        largeMessage.SetActive(true);
        inGameMenu.SetActive(true);
        LocalPlayerManager.Singleton.localPlayer.isDead = true;

    }

    /// <summary>
    /// Destroys objects tagged with "DontDestroy" when returning to the menu scene.
    /// </summary>
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

    /// <summary>
    /// Handles the event when a player picks up a ring.
    /// </summary>
    /// <param playerName="playerName">The playerName of the player who picked up the ring.</param>
    public void OnPlayerPickedUpRing(string playerName)
    {
        ChatManager.Instance.SendMsg(playerName + " collected a ring!", "Wizard");
    }

    /// <summary>
    /// Handles the visualization when a player wins the game.
    /// </summary>
    /// <param playerName="playerName">The playerName of the player who won the game.</param>
    public void VisualizeOnGameWon(string playerName)
    {
        gameStatusSO.gameIsOver = true;
        infoText.text = playerName + " won the game!";
        inGameMenu.SetActive(true);
        largeMessage.SetActive(true);
    }

    /// <summary>
    /// Deactivates all the players in the game.
    /// </summary>
    public void DeactivateAllPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            player.SetActive(false);
        }
    }
}
