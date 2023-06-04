using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerDeath : MonoBehaviour
{
    private PlayerHealth playerHealth;
    private Animator animator;
    private readonly WaitForSeconds waitForSeconds = new(7f);
    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerHealth.OnPlayerKnockdown += OnPlayerKnockdown; //subscribe
        animator = GetComponentInChildren<Animator>();
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        bool test = e.isKnockedDown;
        animator.SetBool("Knockdown", true);

    }
}
