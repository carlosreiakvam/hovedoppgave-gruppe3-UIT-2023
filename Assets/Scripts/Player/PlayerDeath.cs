using System.Collections;
using System.Collections.Generic;
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
        animator.SetBool("Knockdown", true); //Todo: Notify other players and make enemy uninterested as well
        //StartCoroutine(KnockdownRoutine());
        //TODO: connect animation of player going down
        //hide sprite        

        //run CoRoutine with countdown
        //TODO: if player not saved within a certain amount of time:
        //playerDeath.OnPlayerDead -= PlayerDeath_OnPlayerDead; //unsubscribe. 
        //TODO: if saved, raise player backup and increase networkHP via SO

    }

    private IEnumerator KnockdownRoutine()
    {
        yield return waitForSeconds;
        Debug.Log(waitForSeconds.ToString());
        gameObject.SetActive(false);
        gameObject.GetComponentInParent<NetworkObject>().Despawn(true);
    }
}
