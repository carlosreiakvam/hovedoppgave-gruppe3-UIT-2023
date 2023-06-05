using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System;
using System.Diagnostics.CodeAnalysis;

public class EnemyHealth : NetworkBehaviour
{
    private float hitPoints;
    private float startingHP;
    [SerializeField] private Image healthBarVisual;

    /// <summary>
    /// Initializes the hit points and starting HP variables.
    /// </summary>
    private void Start()
    {
        hitPoints = 25;
        startingHP = 25;
    }

    /// <summary>
    /// Custom event arguments for player knockdown event.
    /// </summary>
    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }

    /// <summary>
    /// Method called when an players's sword collides with this enemy.
    /// </summary>
    public void SwordCollision(float damage)
    {
        ApplyDamage(damage);
    }

    /// <summary>
    /// Applies damage to this enemy's health.
    /// </summary>
    private void ApplyDamage(float damage)
    {
        hitPoints -= damage;
        print("applying damage to: " + gameObject.name);
        VisualizeHealthChangeServerRpc(hitPoints, startingHP);

        if (hitPoints <= 0) Despawn();
    }

    /// <summary>
    /// Despawns this enemy.
    /// </summary>
    private void Despawn()
    {
        if (IsServer) NetworkBehaviour.Destroy(gameObject);

        ulong networkId = GetComponent<NetworkObject>().NetworkObjectId;
        SpawnManager.Singleton.RemoveFromSpawnedList(networkId);
    }

    /// <summary>
    /// ServerRpc method to visualize this emey's health change on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void VisualizeHealthChangeServerRpc(float hp, float startHP)
    {
        VisualizeHealthChangeClientRpc(hp, startingHP);
    }

    /// <summary>
    /// ClientRpc method to visualize this enemy's health change on the client.
    /// </summary>
    [ClientRpc]
    private void VisualizeHealthChangeClientRpc(float hp, float startHP) //inform the other clients
    {

        healthBarVisual.fillAmount = hp / startHP;
    }
}
