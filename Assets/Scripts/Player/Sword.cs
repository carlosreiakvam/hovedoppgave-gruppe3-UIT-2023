using UnityEngine;

public class Sword : MonoBehaviour
{
    PlayerHealth playerHealth;
    EnemyHealth enemyHealth;

    /// <summary>
    /// When sword collides with objects, check if player or enemy holds the sword, then check if player or enemy was the one that was hit.
    /// If player hits another player, do nothing.
    /// If enemy hits another enemy, do nothing.
    /// If player hits an enemy, damage the enemy.
    /// If enemy hits a player, damage the player.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponentInParent<Enemy>())
        {
            if (collision.GetComponentInParent<PlayerBehaviour>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
                playerHealth.SwordCollision();
            }
        }
        else if (GetComponentInParent<PlayerBehaviour>())
        {
            if (collision.GetComponentInParent<Enemy>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
                if (GetComponentInParent<PlayerBehaviour>().GetSword())
                {
                    enemyHealth.SwordCollision(5);
                }
                else
                {
                    enemyHealth.SwordCollision(25);
                }
            }
        }
    }
}
