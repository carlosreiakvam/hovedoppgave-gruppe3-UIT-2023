using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    private PlayerHealth playerDeath;
    void Start()
    {
        playerDeath = GetComponent<PlayerHealth>();
        playerDeath.OnPlayerDead += PlayerDeath_OnPlayerDead; //subscribe
    }

    private void PlayerDeath_OnPlayerDead(object sender, System.EventArgs e)
    {
        //TODO: connect animation of player going down
        gameObject.SetActive(false); //hide sprite        
      
        //TODO: if player not saved within a certain amount of time:
        playerDeath.OnPlayerDead -= PlayerDeath_OnPlayerDead; //unsubscribe. 
        //TODO: if saved, raise player backup and increase health via SO

        Destroy(gameObject);
        
    }
}


//In the future we will only unsubscribe after a certain amount of time has passed such that players have time to help the person up again