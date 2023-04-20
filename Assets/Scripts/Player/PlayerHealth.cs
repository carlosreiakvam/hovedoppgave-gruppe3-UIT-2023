using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class PlayerHealth : NetworkBehaviour
{
    public event EventHandler OnPlayerKnockdown; //Publisher of death!
    [SerializeField] private FloatVariable hitPoints;
    [SerializeField] private FloatVariable lightDamageTaken;
    [SerializeField] private bool resetHP;
    [SerializeField] private FloatReference startingHP;
    [SerializeField] private Image healthBarVisual;
    [SerializeField] private NetworkVariable<float> health = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private float timer = 0;
    private readonly float TIME_TO_DAMAGE = 1;

    private void Start()
    {
        if (resetHP)
            hitPoints.SetValue(startingHP);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
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
    private void ApplyDamage()
    {
        if (timer >= TIME_TO_DAMAGE)
        {
            hitPoints.ApplyChange(-lightDamageTaken.Value);
            health.Value = hitPoints.Value / startingHP;
            timer = 0;           
            Debug.Log($"Current hitpoints for player {OwnerClientId + 1}: " + hitPoints.Value);
            if (hitPoints.Value <= 0)
            {
                Debug.Log("DEAD!");
                VizualizeDeathServerRpc();
            }
        }
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
        OnPlayerKnockdown?.Invoke(this, EventArgs.Empty);
    }
}
