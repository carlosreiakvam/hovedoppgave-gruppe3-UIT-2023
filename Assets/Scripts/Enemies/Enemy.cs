using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using UnityEngine.AI;
using System.Linq;
using Mono.CSharp;

public class Enemy : NetworkBehaviour
{
    private Transform target = null;
    float lookingDistance = 10f;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private Vector2 moveDirection = new(0, 0);
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 0;
    private readonly WaitForSeconds waitForSeconds = new(3f);
    private List<Transform> targets = new();

    RaycastHit2D[] hits;

    private Vector2 roamPosition;
    private State state;
    GameObject[] players;


    private enum State
    {
        Looking,
        ChaseTarget,
        PlayerDown,
    }

    private void Awake()
    {
        state = State.Looking;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        players = GameObject.FindGameObjectsWithTag("Player");
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        state = State.Looking; //for the host
        //Debug.Log($"OnPlayerKnockdown callback; Player with ID: {playerID} is knocked down is {e.isKnockedDown}");
        StopAnimationClientRpc(); //Notify the clients to stop the animation
        target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown -= OnPlayerKnockdown; //prevent firing when player is already down
    }


    private void FixedUpdate()
    {
        if (!IsServer) return;

        switch (state)
        {
            default:

            case State.Looking:
                LookForTarget();
                break;

            case State.ChaseTarget:
                ChaseTarget();
                break;

            case State.PlayerDown:
                //StartCoroutine(TakeABreakRoutine()); //Take a break before going back into roaming
                break;
        }
    }

    private IEnumerator TakeABreakRoutine()
    {
        yield return waitForSeconds;
        Debug.Log(waitForSeconds);
        state = State.Looking;
    }

    private void LookForTarget()
    {
        targets.Clear();
        foreach (GameObject player in players)
        {
            float distance = Vector2.Distance(player.transform.position, transform.position);
            if (distance < lookingDistance) targets.Add(player.transform);
        }
        if (targets.Count > 0)
        {
            target = targets[Random.Range(0, players.Length)];
            target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
            state = State.ChaseTarget;
        }
    }

    private void ChaseTarget()
    {
        if (target != null)
        {
            if (Vector2.Distance(transform.position, target.position) >= lookingDistance)
            {
                state = State.Looking;
                animator.SetFloat(SPEED, 0);
                return;
            }


            moveDirection = (target.transform.position - transform.position).normalized;
            transform.Translate(SPEED_VALUE * Time.deltaTime * moveDirection);

            animator.SetFloat(HORIZONTAL, moveDirection.x);
            animator.SetFloat(VERTICAL, moveDirection.y);
            animator.SetFloat(SPEED, moveDirection.sqrMagnitude);

            hits = Physics2D.CapsuleCastAll(transform.position, size, CapsuleDirection2D.Vertical, 0, Vector2.up, 0);
            foreach (RaycastHit2D raycastHit2D in hits)
            {
                if (raycastHit2D.collider.name == "PlayerAnimation")
                {
                    Attack();
                }
            }
        }
    }

    //private void OnTriggerEnter2D(Collider2D collision) //A new target for the enemy!
    //{
    //    if (!IsServer) return;
    //    if (!collision.CompareTag("Player")) return;

    //    if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
    //    {
    //        target = collision.transform;
    //        //target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
    //    }
    //}


    private void StopAnimation()
    {
        state = State.PlayerDown;
        moveDirection = Vector2.zero;
        animator.SetFloat(SPEED, 0);
    }

    [ClientRpc]
    private void StopAnimationClientRpc()
    {
        StopAnimation();
    }



    private void Attack()
    {
        animator.SetTrigger(STEELATTACK);
    }
}