using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponentInParent<Enemy>())
        {
            if (collision.GetComponentInParent<PlayerBehaviour>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                //print("Player hit!");
                playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
                playerHealth.SwordCollision();
            }
        }
        else if (GetComponentInParent<PlayerBehaviour>())
        {
            if (collision.GetComponentInParent<Enemy>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                //print("Enemy hit!");
                enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
                if (GetComponentInParent<PlayerBehaviour>().GetSword())
                {
                    enemyHealth.SwordCollision(5);
                }
                else
                {
                    enemyHealth.SwordCollision(25);
                }
                //enemyHealth.SwordCollision();
            }
        }
    }
}
