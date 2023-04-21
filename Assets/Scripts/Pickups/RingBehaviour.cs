using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RingBehaviour : NetworkBehaviour
{
    GameManager gameManager;
    private void Start()
    {
        gameManager = GetComponentInParent<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        int playerId = collision.GetInstanceID();
        gameManager.OnPlayerCollectedRing(playerId);

        Debug.Log("Player with instance playerId " + playerId + " collided with ring");
        Debug.Log("playerIdHasRing is: " + gameManager.playerIdHasRing);

        NetworkObject.Despawn(true);
    }

}

