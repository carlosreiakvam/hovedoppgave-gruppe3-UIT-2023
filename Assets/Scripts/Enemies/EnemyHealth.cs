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
        //if (!IsOwner) return;
        ApplyDamage(damage);
        VisualizeHealthChangeServerRpc(hitPoints, startingHP);
    }

    private void ApplyDamage(float damage)
    {
        hitPoints -= damage;
        print("applying damage to: " + gameObject.name);
        if (hitPoints <= 0)
        {
            //VizualizeDeathServerRpc();
            NetworkBehaviour.Destroy(gameObject);
        }
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
