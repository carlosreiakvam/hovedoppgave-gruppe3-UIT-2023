using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System;
using System.Diagnostics.CodeAnalysis;
using Mono.CSharp;

public class EnemyHealth : NetworkBehaviour
{
    private float hitPoints;
    private float startingHP;
    [SerializeField] private Image healthBarVisual;

    private void Start()
    {
        hitPoints = 25;
        startingHP = 25;
    }

    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }

    public void SwordCollision(float damage)
    {
        ApplyDamage(damage);
    }

    private void ApplyDamage(float damage)
    {
        hitPoints -= damage;
        print("applying damage to: " + gameObject.name);
        VisualizeHealthChangeServerRpc(hitPoints, startingHP);

        if (hitPoints <= 0) Despawn();
    }
    private void Despawn()
    {
        if (IsServer) NetworkBehaviour.Destroy(gameObject);

        ulong networkId = GetComponent<NetworkObject>().NetworkObjectId;
        SpawnManager.Singleton.RemoveFromSpawnedList(networkId);
    }


    [ServerRpc(RequireOwnership = false)]
    private void VisualizeHealthChangeServerRpc(float hp, float startHP)
    {
        VisualizeHealthChangeClientRpc(hp, startingHP);
    }

    [ClientRpc]
    private void VisualizeHealthChangeClientRpc(float hp, float startHP) //inform the other clients
    {

        healthBarVisual.fillAmount = hp / startHP;
    }
}
