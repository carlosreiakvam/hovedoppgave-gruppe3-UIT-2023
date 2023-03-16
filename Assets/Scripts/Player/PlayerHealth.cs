using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private UnityEvent onPlayerDeath;
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
    /// The longer the object colliders are touching
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
            }
        }

        VisualizeDamageServerRpc();

    }
    private void ApplyDamage()
    {

        if (timer >= TIME_TO_DAMAGE)
        {
            hitPoints.ApplyChange(-lightDamageTaken.Value);
            healthBarVisual.fillAmount = hitPoints.Value / startingHP;
            health.Value = healthBarVisual.fillAmount; //set the networkvariable
            timer = 0; //reset             
            Debug.Log($"Current hitpoints for player {OwnerClientId + 1}: " + hitPoints.Value);
            if (hitPoints.Value <= 0)
            {
                Debug.Log("DEAD!");
                onPlayerDeath.Invoke();
            }
        }
    }


    [ServerRpc(RequireOwnership = false)] //All clients may rpc the server
    private void VisualizeDamageServerRpc()
    {
        VisualizeDamageClientRpc(); //rpc damage taken to server
    }


    [ClientRpc]
    private void VisualizeDamageClientRpc()
    {
        healthBarVisual.fillAmount = health.Value; //inform the other clients
    }
}
