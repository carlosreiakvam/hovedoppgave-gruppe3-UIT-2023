using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public class HealthPowerup : NetworkBehaviour
{
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        gameObject.SetActive(false);
        GameObject player = collision.gameObject;
        PlayerHealth playerHealth = player.GetComponentInChildren<PlayerHealth>();
        playerHealth.AddHealth();
        Debug.Log("HP PowerUp Collided with player: " + collision.gameObject.GetInstanceID());
    }
}
