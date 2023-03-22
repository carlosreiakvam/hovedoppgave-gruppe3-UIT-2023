using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    void Start()
    {
        PlayerHealth playerDeath = GetComponentInParent<PlayerHealth>();
        
        playerDeath.OnPlayerDead += PlayerDeath_OnPlayerDead; //subscribe
    }

    private void PlayerDeath_OnPlayerDead(object sender, System.EventArgs e)
    {
        
        gameObject.SetActive(false); //only hides sprite, not HUD
        
        PlayerHealth playerDeath = GetComponentInParent<PlayerHealth>();
        GetComponentInParent<PlayerHealth>().gameObject.SetActive(false); //hides HUD
        //if player not saved within a certain amount of time:
            playerDeath.OnPlayerDead -= PlayerDeath_OnPlayerDead; //unsubscribe. In the future we will only unsubscribe after a certain amount of time has passed such that players have time to help the person up again
    }
}
