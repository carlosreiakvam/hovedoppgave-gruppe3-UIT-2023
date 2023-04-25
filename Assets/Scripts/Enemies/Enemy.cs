using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;

public class Enemy : NetworkBehaviour
{
    private Transform target = null;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private Vector2 moveDirection = new(0, 0);
    private bool playerDown = false;
    private int playerID;
    private const string STEELATTACK = "SteelAttack";

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

        if (collision.GetComponentInParent<PlayerBehaviour>())
        {
            playerDown = false;
            target = collision.transform;
            playerID = (int) collision.GetComponentInParent<PlayerBehaviour>().OwnerClientId;
            Debug.Log($"New Target, With ID: " + playerID);
            //target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
            try
            {
                target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
            }
            catch (Exception e)
            {
                target.GetComponentInParent<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
            }
        }
    }

    private void Attack()
    {
        networkAnimator.SetTrigger(STEELATTACK);
    }
}
