using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System;
using System.Diagnostics.CodeAnalysis;
using Mono.CSharp;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private FloatVariable hitPoints;
    [SerializeField] private FloatVariable lightDamageTaken;
    [SerializeField] private bool resetHP;
    [SerializeField] private FloatReference startingHP;
    [SerializeField] private Image healthBarVisual;
    [SerializeField] private FloatVariable healthPowerUpAmount;
    public event EventHandler<OnPlayerKnockdownEventArgs> OnPlayerKnockdown; //Publisher of death!

    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }

    private void Start()
    {
        if (resetHP)
            hitPoints.SetValue(startingHP);
    }

    public void SwordCollision()
    {
        if (!IsOwner) return;
        ApplyDamage();
        VisualizeHealthChangeServerRpc(hitPoints.Value, startingHP.Value);
    }

    private void ApplyDamage()
    {
        if (IsLocalPlayer)
        {
            hitPoints.ApplyChange(-lightDamageTaken.Value);
            print("applying damage to: " + gameObject.name);
            if (hitPoints.Value <= 0)
            {
                VizualizeDeathServerRpc();
            }
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocalPlayer)
        {
            if (!collision.CompareTag("HealthPowerUp")) return;

            Debug.Log("OnTriggerEnter2D in PlayerHealth: " + collision.name);

            if (AddHealth()) SpawnManager.Singleton.DespawnObjectServerRpc(collision.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }
    public bool AddHealth()
    {
        if (hitPoints.Value < 0 || hitPoints.Value == startingHP) return false;

        hitPoints.ApplyChange(healthPowerUpAmount.Value);

        if (hitPoints.Value >= startingHP /*- healthPowerUpAmount*/) //might overshoot
        {
            hitPoints.Value = startingHP; // Clamp at max networkHP
        }

        VisualizeHealthChangeServerRpc(hitPoints.Value, startingHP.Value);
        return true;

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

    [ServerRpc(RequireOwnership = false)]
    private void VizualizeDeathServerRpc()
    {
        VizualizeDeathClientRpc();
    }

    [ClientRpc]
    private void VizualizeDeathClientRpc()
    {
        OnPlayerKnockdown?.Invoke(this, new OnPlayerKnockdownEventArgs { isKnockedDown = true });
    }
}
