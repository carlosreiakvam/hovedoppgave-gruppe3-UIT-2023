using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void OnPlayerKnockdown(object sender, System.EventArgs e)
    {
        animator.SetBool("Knockdown", true);
        //TODO: connect animation of player going down
        //gameObject.SetActive(false); //hide sprite        
      
        //TODO: if player not saved within a certain amount of time:
        //p 
        //TODO: if saved, raise player backup and increase health via SO

        //Destroy(gameObject);
        
    }

    //In the future we will only unsubscribe after a certain amount of time has passed 
    //such that players have time to help the person up again
    private IEnumerator CountToDeathRoutine()
    {
        yield return waitForSeconds;
        Debug.Log(waitForSeconds.ToString());
        gameObject.SetActive(false);
        playerHealth.OnPlayerKnockdown -= OnPlayerKnockdown; //unsubscribe.
    }
}


