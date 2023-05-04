using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;

public class Enemy :  NetworkBehaviour
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
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 0;

    RaycastHit2D[] hits;

    private void Update()
    {
        if (timeLeftToAttack > 0)
        {
            timeLeftToAttack -= Time.deltaTime;
            if (timeLeftToAttack < 0)
            {
                timeLeftToAttack = 0;
            }
        }
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        Debug.Log($"OnPlayerKnockdown callback; Player with ID: {playerID} is knocked down is {e.isKnockedDown}");
        //StopAnimationServerRpc(); //This is for the server
        StopAnimationClientRpc(); //Notify the clients to stop the animation
        playerDown = true; //for this server
    }

    //private void StopAnimationServerRpc()
    //{
    //    throw new NotImplementedException();
    //}

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

            // Physics2D.RaycastAll(transform.position, Vector2.up, 0.5f)
            hits = Physics2D.CapsuleCastAll(transform.position, size, CapsuleDirection2D.Vertical, 0, Vector2.up, 0);
            foreach (RaycastHit2D raycastHit2D in hits)
            {
                if (raycastHit2D.collider.name == "PlayerAnimation")
                {
                    if (timeLeftToAttack == 0)
                    {
                        Attack();
                        timeLeftToAttack = 1;
                    }
                }
            }
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
        if (!collision.CompareTag("Player")) return;

        if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
        {
            playerDown = false;
            target = collision.transform;
            playerID = (int)collision.GetComponentInParent<PlayerBehaviour>().OwnerClientId;
            Debug.Log($"New Target, With ID: " + playerID);
            target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
        }
    }

    private void Attack()
    {
        animator.SetTrigger(STEELATTACK);
    }
}