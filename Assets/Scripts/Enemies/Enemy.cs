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
    private int playerID;
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 0;
    private const float ROAMING_SPEED = 1f;
    private readonly WaitForSeconds waitForSeconds = new(3f);

    RaycastHit2D[] hits;

    private Vector2 startingPosition;
    private Vector2 roamPosition;
    private State state;

    private enum State
    {
        Roaming,
        ChaseTarget,
        PlayerDown,
    }

    private void Awake()
    {
        state = State.Roaming;
    }

    private void Start()
    {
        startingPosition = transform.position;
        roamPosition = GetRoamingPosition();
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        state = State.PlayerDown;
        //Debug.Log($"OnPlayerKnockdown callback; Player with ID: {playerID} is knocked down is {e.isKnockedDown}");
        StopAnimationClientRpc(); //Notify the clients to stop the animation
        target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown -= OnPlayerKnockdown; //prevent firing when player is already down
    }

    private Vector3 GetRoamingPosition()
    {
        return startingPosition + GetRandomDir() * Random.Range(5f, 20f);
    }

    private Vector2 GetRandomDir()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void Update()
    {
        if (!IsServer) return;

        switch (state)
        {
            default:

            case State.Roaming:
                Debug.Log("Current State:" + state.ToString());

                moveDirection = (roamPosition - (Vector2)transform.position).normalized;
                transform.Translate(ROAMING_SPEED * Time.deltaTime * moveDirection);

                //Debug.Log("Distance left to target:" + Vector2.Distance(transform.position, roamPosition));

                if (Vector2.Distance(transform.position, roamPosition) < 1)
                {
                    //reached roam position
                    roamPosition = GetRoamingPosition();
                }

                animator.SetFloat(HORIZONTAL, moveDirection.x);
                animator.SetFloat(VERTICAL, moveDirection.y);
                animator.SetFloat(SPEED, moveDirection.sqrMagnitude);

                FindTarget(); //Look for a target!

                break;

            case State.ChaseTarget:
                Debug.Log("Current State:" + state.ToString());
                if (target != null)
                {
                    if (timeLeftToAttack > 0)
                    {
                        timeLeftToAttack -= Time.deltaTime;
                        if (timeLeftToAttack < 0)
                        {
                            timeLeftToAttack = 0;
                        }
                    }

                    moveDirection = (target.transform.position - transform.position).normalized;
                    transform.Translate(SPEED_VALUE * Time.deltaTime * moveDirection);

                    animator.SetFloat(HORIZONTAL, moveDirection.x);
                    animator.SetFloat(VERTICAL, moveDirection.y);
                    animator.SetFloat(SPEED, moveDirection.sqrMagnitude);
                }
                break;

            case State.PlayerDown: //dummy state
                Debug.Log("Current State:" + state.ToString());
                moveDirection = Vector2.zero;
                
                StartCoroutine(TakeABreakRoutine()); //Take a break before going back into roaming

                break;
        }
    }

    private IEnumerator TakeABreakRoutine()
    {
        yield return waitForSeconds;
        Debug.Log(waitForSeconds);
        state = State.Roaming;
    }



    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (target != null)
        {
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

    private void FindTarget()
    {
        if (!IsServer || target == null) return;

        float targetRange = 2;
        if (Vector2.Distance(transform.position, target.position) < targetRange)
        {
            //Debug.Log("Found a new Target! State before update state:" + state.ToString());
            state = State.ChaseTarget;
            //Debug.Log("Found a new Target! State after update state:" + state.ToString());
        }
    }

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

    private void OnCollisionEnter2D(Collision2D collision) //colliding with a "hard" gameobject. Find another path!
    {
        if (!IsServer) return;

        if (collision.collider.CompareTag("StaticColliders"))
        {
            //Debug.Log("It is a static collider");
            roamPosition = Mathf.Abs(GetRandomDir().x) * Mathf.Abs(GetRandomDir().y) * -roamPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //A new target for the enemy!
    {
        if (!IsServer) return;
        if (!collision.CompareTag("Player")) return;

        if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
        {
            target = collision.transform;
            target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
        }
    }

    private void Attack()
    {
        animator.SetTrigger(STEELATTACK);
    }
}