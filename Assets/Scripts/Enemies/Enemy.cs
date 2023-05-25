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
    readonly float lookingDistance = 5f;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    private Vector2 moveDirection = new(0, 0);
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 1;
    private int indexOfChasedPlayer = 0;

    RaycastHit2D[] hits;

    private State state;
    private List<GameObject> players;

    // A minimum and maximum time delay for taking a decision, choosing a direction to move in
    public Vector2 decisionTime = new(0.1f, 4.0f);
    internal float decisionTimeCount = 0;

    // The possible directions that the object can move int, right, left, up, down, and zero for staying in place. I added zero twice to give a bigger chance if it happening than other directions
    internal Vector2[] moveDirections = new Vector2[] { Vector2.right, Vector2.left, Vector2.down, Vector2.up };
    internal int currentMoveDirection;

    private enum State
    {
        Roaming,
        ChaseTarget,
        PlayerDown
    }

    private void Awake()
    {
        state = State.Roaming;
    }

    private void Update()
    {
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

    private void Start()
    {
        // Set a random time delay for taking a decision ( changing direction, or standing in place for a while )
        decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);

        // Choose a movement direction, or stay in place
        ChooseMoveDirection();
    }

    void ChooseMoveDirection()
    {
        // Choose whether to move sideways or up/down
        currentMoveDirection = Mathf.FloorToInt(Random.Range(0, moveDirections.Length));
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        players = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    private int counter;
    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        //counter++;
        //Debug.Log("counter" + counter);

        //check if the knocked down player is the same as the chased player
        //Debug.Log("player.Any()" + players.Any());

        if (players.Any() && players != null && indexOfChasedPlayer >= 0 && !e.isKnockedDown)
        {
            //Debug.Log("players[indexOfChasedPlayer].locpos" + (players[indexOfChasedPlayer]).transform.localPosition);
            //Debug.Log("target.transform.locpos" + target.transform.localPosition);
            if (players[indexOfChasedPlayer].transform == target.transform)
            {
                players.RemoveAt(indexOfChasedPlayer);
                target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown -= OnPlayerKnockdown;
            }
        }
        else
        {
            target = null;
        }
        
        StopAnimationClientRpc(); //Notify the clients to stop the animation
        state = State.PlayerDown;
        
    }


    private void FixedUpdate()
    {
        if (!IsServer) return;

        switch (state)
        {
            case State.Roaming:
                if (players.Any() && players != null)
                {
                    moveDirection = moveDirections[currentMoveDirection];
                    transform.Translate(SPEED_VALUE * Time.deltaTime * moveDirection);

                    animator.SetFloat(HORIZONTAL, moveDirection.x);
                    animator.SetFloat(VERTICAL, moveDirection.y);
                    animator.SetFloat(SPEED, moveDirection.sqrMagnitude);

                    if (decisionTimeCount > 0) decisionTimeCount -= Time.deltaTime;
                    else
                    {
                        // Choose a random time delay for taking a decision ( changing direction, or standing in place for a while )
                        decisionTimeCount = Random.Range(decisionTime.x, decisionTime.y);

                        // Choose a movement direction
                        ChooseMoveDirection();
                        SearchForTarget();
                    }
                }
                break;

            case State.PlayerDown:
                StartCoroutine(BreakBeforeRoaming());
                break;

            case State.ChaseTarget:
                ChaseTarget();
                break;
        }
    }

    private IEnumerator BreakBeforeRoaming()
    {
        yield return new WaitForSeconds(3f);
        state = State.Roaming;
    }

    private void SearchForTarget()
    {
        if (players.Any() && players != null && indexOfChasedPlayer >= 0)
        {
            foreach (GameObject player in players)
            {
                if (Vector2.Distance(player.transform.position, transform.position) < lookingDistance)
                {
                    indexOfChasedPlayer = players.IndexOf(player);
                    state = State.ChaseTarget;
                    target = player.transform;
                    target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
                }
            }
        }
    }

    private void ChaseTarget()
    {
        if (players.Any() && players != null)
        {

            if (target != null)
            {
                if (Vector2.Distance(transform.position, target.position) >= lookingDistance)
                {
                    state = State.Roaming;
                    return;
                }

                moveDirection = (target.transform.position - transform.position).normalized;
                transform.Translate(SPEED_VALUE * Time.deltaTime * moveDirection);

                animator.SetFloat(HORIZONTAL, moveDirection.x);
                animator.SetFloat(VERTICAL, moveDirection.y);
                animator.SetFloat(SPEED, moveDirection.sqrMagnitude);

                if (timeLeftToAttack == 0)
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
        }
    }

    public LayerMask filter;
    private void HandleHardCollision()
    {
        Physics2D.queriesHitTriggers = false; //must be reset to true when triggers are to have an effect
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, 0.3f, filter);

        //hit.collider.GetComponent<Collider2D>().name;
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("StaticColliders"))
            {
                Debug.Log("Collider name: " + hit.collider.name);
                //Debug.DrawRay(transform.position, moveDirection);
                moveDirection = -moveDirection;
            }
        }
    }

    //react to the circle collider to pick new targets within range
    private void OnTriggerEnter2D(Collider2D collision) //A new target for the enemy!
    {
        if (!IsServer) return;
        if (players.Any() && players != null)
        {
            if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
            {
                target = collision.transform;
                //playerID = (int)collision.GetComponentInParent<PlayerBehaviour>().OwnerClientId;
                //Debug.Log($"New Target, With ID: " + playerID);
                //target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
            }
        }
    }

    private void StopAnimation()
    {
        if (players?.Any() != true) // Handle null or empty list
        {
            target = null;
        }
        moveDirection = Vector2.zero;
        animator.SetFloat(SPEED, 0);
        //state = State.PlayerDown;
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