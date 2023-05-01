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
    private const float START_HP = 100f;


    float healthPowerUpAmount = 10f;
    [SerializeField] private bool resetHP;
    [SerializeField] private Image healthBarVisual;
    NetworkVariable<float> networkHP = new(START_HP);

    private float lightDamageTaken = 3f;
    public bool IsKnockedDown { get; private set; } = false;

    public event EventHandler<OnPlayerKnockdownEventArgs> OnPlayerKnockdown; //Publisher of death!

    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }


    public void SwordCollision()
    {
        if (IsOwner) ApplyDamage();
    }

    private void ApplyDamage()
    {
        float newHP = networkHP.Value;
        print("applying damage to: " + gameObject.name);
        newHP -= lightDamageTaken;
        if (newHP <= 0)
        {
            newHP = 0;
            IsKnockedDown = true;
            VizualizeDeathServerRpc();
        }
        ChangeHealthServerRpc(newHP);
    }
    public void AddHealth()
    {
        float newHP = networkHP.Value;
        if (newHP == START_HP) return; // return if already at max networkHP
        newHP += healthPowerUpAmount; // Add networkHP
        if (newHP >= START_HP) newHP = START_HP; // Clamp at max networkHP
        ChangeHealthServerRpc(newHP); // Inform the other clients
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeHealthServerRpc(float newHP)
    {
        //if (!IsServer) return; // remove?
        networkHP.Value = newHP;
        VisualizeDamageClientRpc(newHP);
    }


    [ClientRpc]
    private void VisualizeDamageClientRpc(float newHP) //inform the other clients
    {
        float hbVisual = newHP / START_HP;
        healthBarVisual.fillAmount = hbVisual;
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
