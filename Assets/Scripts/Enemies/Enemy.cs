using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using UnityEngine.AI;
using System.Linq;

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
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 0;
    private readonly WaitForSeconds waitForSeconds = new(3f);

    NavMeshAgent agent;

    RaycastHit2D[] hits;

    private Vector2 startingPosition;
    private Vector2 roamPosition;
    private State state;

    //public Vector2Int[,] validRoamLocations;
    //private int validRoamLocationsCount = 0;

    private enum State
    {
        Roaming,
        ChaseTarget,
        PlayerDown,
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        state = State.Roaming;
    }

    private void Start()
    {
        startingPosition = transform.position;
        roamPosition = GetRoamingPosition();
        agent.SetDestination(roamPosition);
    }

    private Vector2 GetRoamingPosition()
    {
        Vector2 roamVec;
        if (transform.position.x < 50)
            roamVec = SpawnManager.Singleton.GetEmptyTile(searchRange: 1, EnvironmentEnums.Outdoor, excludedMidAreaSideLength: 10);
        else
            roamVec = SpawnManager.Singleton.GetEmptyTile(searchRange: 1, EnvironmentEnums.Cave);

        return roamVec;
    }

    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        state = State.PlayerDown;
        //Debug.Log($"OnPlayerKnockdown callback; Player with ID: {playerID} is knocked down is {e.isKnockedDown}");
        StopAnimationClientRpc(); //Notify the clients to stop the animation
        target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown -= OnPlayerKnockdown; //prevent firing when player is already down
    }

    private void Update()
    {
        if (!IsServer) return;

        if (state == State.ChaseTarget)
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


        switch (state)
        {
            default:

            case State.Roaming:
                moveDirection = (roamPosition - (Vector2)transform.position).normalized;
                agent.SetDestination(roamPosition);
                //transform.Translate(ROAMING_SPEED * Time.deltaTime * moveDirection);

                Debug.Log("Distance left to target:" + Vector2.Distance(transform.position, roamPosition));
                if (Vector2.Distance(transform.position, roamPosition) < agent.stoppingDistance)
                {
                    //reached roam position, get new
                    roamPosition = GetRoamingPosition();

                }

                animator.SetFloat(HORIZONTAL, moveDirection.x);
                animator.SetFloat(VERTICAL, moveDirection.y);
                animator.SetFloat(SPEED, moveDirection.sqrMagnitude);

                FindTarget(); //Look for a target!

                break;

            case State.ChaseTarget:
                if (target != null)
                {

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
                break;

            case State.PlayerDown: //dummy state
                                   //Debug.Log("Current State:" + state.ToString());

                StartCoroutine(TakeABreakRoutine()); //Take a break before going back into roaming

                break;
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
        if (!IsServer || collision.collider.CompareTag("Player")) return;

        agent.SetDestination(roamPosition);

        //Debug.Log("It is a static collider");
        //TODO: find a new random position
        //roamPosition = Mathf.Abs(GetRandomDir().x) * Mathf.Abs(GetRandomDir().y) * -roamPosition;
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