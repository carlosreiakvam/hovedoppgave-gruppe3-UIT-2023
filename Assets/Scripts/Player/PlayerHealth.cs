using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private FloatVariable hitPoints;
    [SerializeField] private FloatVariable lightDamageTaken;
    [SerializeField] private bool resetHP;
    [SerializeField] private FloatReference startingHP;
    [SerializeField] private Image healthBarVisual;
    [SerializeField] private FloatVariable healthPowerUpAmount;
    public event EventHandler<OnPlayerKnockdownEventArgs> OnPlayerKnockdown; //Publisher of death!

    /// <summary>
    /// Custom event arguments for player knockdown event.
    /// </summary>
    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }

    private void Start()
    {
        if (resetHP)
            hitPoints.SetValue(startingHP);
    }

    /// <summary>
    /// Called when a sword collision occurs with the player.
    /// </summary>
    public void SwordCollision()
    {
        if (!IsOwner) return;
        ApplyDamage();
        VisualizeHealthChangeServerRpc(hitPoints.Value, startingHP.Value);
    }

    /// <summary>
    /// Applies damage to the player's health.
    /// </summary>
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

    /// <summary>
    /// Called when the player triggers a 2D collider.
    /// </summary>
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsLocalPlayer)
        {
            if (!collision.CompareTag("HealthPowerUp")) return;

            Debug.Log("OnTriggerEnter2D in PlayerHealth: " + collision.name);

            if (AddHealth()) SpawnManager.Singleton.DespawnObjectServerRpc(collision.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    /// <summary>
    /// Adds health to the player.
    /// </summary>
    public bool AddHealth()
    {
        if (hitPoints.Value < 0 || hitPoints.Value == startingHP) return false;

        hitPoints.ApplyChange(healthPowerUpAmount.Value);

        if (hitPoints.Value >= startingHP) //might overshoot
        {
            hitPoints.Value = startingHP; // Clamp at max networkHP
        }

        VisualizeHealthChangeServerRpc(hitPoints.Value, startingHP.Value);
        return true;

    }

    /// <summary>
    /// ServerRpc method to visualize health change on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void VisualizeHealthChangeServerRpc(float hp, float startHP)
    {
        VisualizeHealthChangeClientRpc(hp, startingHP);
    }

    /// <summary>
    /// ClientRpc method to visualize health change on the client.
    /// </summary>
    [ClientRpc]
    private void VisualizeHealthChangeClientRpc(float hp, float startHP) //inform the other clients
    {

        healthBarVisual.fillAmount = hp / startHP;
    }

    /// <summary>
    /// ServerRpc method to visualize player death on the server.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void VizualizeDeathServerRpc()
    {
        VizualizeDeathClientRpc();
    }

    /// <summary>
    /// ClientRpc method to visualize player death on the client.
    /// </summary>
    [ClientRpc]
    private void VizualizeDeathClientRpc()
    {
        OnPlayerKnockdown?.Invoke(this, new OnPlayerKnockdownEventArgs
        {
            isKnockedDown = true
        });

    }
}
