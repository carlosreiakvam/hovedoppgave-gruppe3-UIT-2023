using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponent<Enemy>())
        {
            if (collision.GetComponent<PlayerBehaviour>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                print("Player hit!");
            }
        }
        else if (GetComponent<PlayerBehaviour>())
        {
            if (collision.GetComponent<Enemy>() && collision.GetType().Name == "CapsuleCollider2D")
            {
                print("Enemy hit! ");
            }
        }
    }
}
