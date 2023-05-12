using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/GameStatusSO", order = 1)]
public class GameStatusSO : ScriptableObject
{
    [HideInInspector]
    public Dictionary<string, string> playersDictionary = new();

    [HideInInspector]
    public List<Player> lobbyPlayers = new();

    [HideInInspector]
    public bool gameIsOver = false;

    [HideInInspector]
    public int playerIdHasRing = -1;

    [HideInInspector]
    public bool isAndroid = false;

    [HideInInspector]
    public bool isWindows = false;

    [HideInInspector]
    public Vector2 caveDoorForest = new Vector2(25, 23);

    [HideInInspector]
    public Vector2 caveDoorInCave = new Vector2(96f, 4.4f);

    public void Reset()
    {
        playersDictionary.Clear();
        lobbyPlayers.Clear();
        gameIsOver = false;
        playerIdHasRing = -1;
        isAndroid = false;
        isWindows = false;
        caveDoorForest = new Vector2(25, 23);
        //caveDoorInCave = new Vector2(96f, 4.4f);
    }
}

