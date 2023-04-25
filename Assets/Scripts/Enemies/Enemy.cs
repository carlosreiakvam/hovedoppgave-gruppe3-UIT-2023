using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    private Transform target = null;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    [SerializeField] private Animator animator;
    private Vector2 moveDirection = new(0, 0);
    private bool playerDown = false;
    private int playerID;

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        Debug.Log($"OnPlayerKnockdown callback; Player with ID: {playerID} is knocked down is {e.isKnockedDown}");
        StopAnimationClientRpc();
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (target != null && !playerDown)
        {
            moveDirection = (target.transform.position - transform.position).normalized;
            transform.Translate(SPEED_VALUE * Time.fixedDeltaTime * moveDirection);
            animator.SetFloat(HORIZONTAL, moveDirection.x);
            animator.SetFloat(VERTICAL, moveDirection.y);
            animator.SetFloat(SPEED, moveDirection.sqrMagnitude);
        }
    }

    private void StopAnimation()
    {
        animator.SetFloat(SPEED, 0);
        playerDown = true;
    }

    [ClientRpc]
    private void StopAnimationClientRpc()
    {
        StopAnimation();
    }

    private void OnTriggerEnter2D(Collider2D collision) //A new target for the enemy!
    {
        if (!IsServer) return;

        if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
        {
            playerDown = false;
            target = collision.transform;
            playerID = (int) collision.GetComponentInParent<PlayerBehaviour>().OwnerClientId;
            Debug.Log($"New Target, With ID: " + playerID);
            target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
        }
        //else the enemy is hit by a weapon attack. Do stuff to the enemy when it is hit by sword

        
    }

    //private void OnCollisionExit2D(Collision collision)
    //{
        
    //}
}
