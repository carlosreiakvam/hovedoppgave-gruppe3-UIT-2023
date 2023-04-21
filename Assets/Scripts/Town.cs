using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Town : MonoBehaviour
{
    GameManager gameManager;
    TextMeshPro wonText;
    GameObject gameUIGO;
    private void Start()
    {
        gameManager = GetComponentInParent<GameManager>();
        //GameObject parent = transform.parent.gameObject;    
        //wonText = gameObject.GetComponentInParent<TextMeshPro>();
        Transform parent = transform.parent;
        Transform grandfather = parent?.parent;
        if (grandfather == null) Debug.Log("no grandfather");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        int playerId = collision.GetInstanceID();
        Debug.Log("Player with instance playerId " + playerId + " collided with town");

        if (playerId == gameManager.playerIdHasRing)
        {
            gameManager.OnGameWon();
        }
    }
}
