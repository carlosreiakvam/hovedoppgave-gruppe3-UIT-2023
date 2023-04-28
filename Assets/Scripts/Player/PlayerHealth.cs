using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System;
using System.Diagnostics.CodeAnalysis;

public class PlayerHealth : NetworkBehaviour
{
    public event EventHandler<OnPlayerKnockdownEventArgs> OnPlayerKnockdown; //Publisher of death!

    public class OnPlayerKnockdownEventArgs : EventArgs
    {
        public bool isKnockedDown = false;
    }
    [SerializeField] private FloatVariable hitPoints;
    [SerializeField] private FloatVariable lightDamageTaken;
    [SerializeField] private FloatVariable healthPowerUpAmount;
    [SerializeField] private bool resetHP;
    [SerializeField] private FloatReference startingHP;
    [SerializeField] private Image healthBarVisual;
    [SerializeField] private NetworkVariable<float> health = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool IsKnockedDown { get; private set; } = false;



    //public NetworkVariable<bool>  IsKnockedDown { get; private set; } = new(false,
    //    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float timer = 0;
    private readonly float TIME_TO_DAMAGE = 1;



    private void Start()
    {
        if (resetHP)
            hitPoints.SetValue(startingHP);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        print("enter: " + collision.gameObject.name);
        if (IsLocalPlayer)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                timer = TIME_TO_DAMAGE;
            }
        }
    }

    /// <summary> 
    /// The longer the object colliders 
    /// for enemy and player are touching
    /// more damage is given.
    /// </summary>
    /// <param name="collision">Who or what is dealing damage</param>
    void OnCollisionStay2D(Collision2D collision)
    {
        print("stay: " + collision.gameObject.name);
        if (IsLocalPlayer)
        {
            timer += Time.deltaTime; //deltatime is the time passed since previous frame

            if (collision.gameObject.CompareTag("Enemy"))
            {
                ApplyDamage();
                VisualizeDamageServerRpc();
            }
        }
    }


    public void SwordCollision()
    {
        timer = TIME_TO_DAMAGE;
        timer += Time.deltaTime; //deltatime is the time passed since previous frame
        ApplyDamage();
        VisualizeDamageServerRpc();
    }
    private void ApplyDamage()
    {
        print("applying damage to: " + gameObject.name);
        if (timer >= TIME_TO_DAMAGE)
        {
            hitPoints.ApplyChange(-lightDamageTaken.Value);
            health.Value = hitPoints.Value / startingHP;
            timer = 0;
            if (hitPoints.Value <= 0)
            {
                IsKnockedDown = true;
                VizualizeDeathServerRpc();
                //ToggleKnockDownServerRpc();
            }
        }
    }
    public void AddHealth()
    {
        if (hitPoints.Value == startingHP.Value) return;

        float newHP = hitPoints.Value + healthPowerUpAmount.Value;
        if (newHP >= startingHP) newHP = startingHP;
        hitPoints.Value = newHP;

        health.Value = hitPoints.Value / startingHP;
        VisualizeDamageServerRpc();
        Debug.LogWarning("HP IS NOW: " + health.Value);
    }

    [ServerRpc(RequireOwnership = false)] //false = All clients may rpc the server
    private void VisualizeDamageServerRpc() //Hey Server, run this code!
    {
        VisualizeDamageClientRpc();
    }

    [ClientRpc] //only server
    private void VisualizeDamageClientRpc() //inform the other clients
    {
        healthBarVisual.fillAmount = health.Value;
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

    //[ServerRpc(RequireOwnership = false)]
    //public void ToggleKnockDownServerRpc()
    //{
    //    // this will cause a replication over the network
    //    // and ultimately invoke `OnValueChanged` on receivers
    //    ToggleKnockDownClientRpc();
    //    //OnPlayerKnockdown?.Invoke(this, new OnPlayerKnockdownEventArgs { isKnockedDown = true });
    //}

    //[ClientRpc]
    //private void ToggleKnockDownClientRpc()
    //{
    //    IsKnockedDown.Value = !IsKnockedDown.Value;
    //}

}
