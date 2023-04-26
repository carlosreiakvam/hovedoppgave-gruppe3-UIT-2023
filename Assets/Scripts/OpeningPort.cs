using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class OpeningPort : MonoBehaviour
{
    GameManager gameManager;
    TextMeshPro wonText;
    GameObject gameUIGO;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        int playerId = collision.GetInstanceID();
        Debug.Log("Player with instance playerId " + playerId + " collided with opening port");

        if (playerId == GameManager.Singleton.networkedPlayerIdHasRing.Value)
        {
            GameManager.Singleton.OnGameWonServerRpc();
        }
    }
}
