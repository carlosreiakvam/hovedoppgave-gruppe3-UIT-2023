using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;
using System.Linq;
using System;

/// <summary>
/// Represents an enemy character controlled by the server.
/// </summary>
public class Enemy : NetworkBehaviour
{
    private Transform target = null;
    private readonly float lookingDistance = 6f;
    private readonly float stopChasingDistance = 10f;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private NetworkAnimator networkAnimator;

    private Vector2 moveDirection = new Vector2(0, 0);
    private Vector2 size = new Vector2(0.5087228f * 2.2f, 0.9851828f * 1.2f);
    private const string STEELATTACK = "SteelAttack";
    private float timeLeftToAttack = 1;
    private int indexOfChasedPlayer = 0;

    RaycastHit2D[] hits;

    private State state;
    private List<GameObject> players;

    /// <summary>
    /// A minimum and maximum time delay for taking a decision.
    /// </summary>
    public Vector2 decisionTime = new(0.1f, 4.0f);

    internal float decisionTimeCount = 0;

    /// <summary>
    /// The possible directions that the object can move in.
    /// </summary>
    internal Vector2[] moveDirections = new Vector2[] { Vector2.right, Vector2.left, Vector2.down, Vector2.up };

    internal int currentMoveDirection;

    /// <summary>
    /// States that the enemy transitions between
    /// </summary>
    private enum State
    {
        Roaming,
        ChaseTarget,
        PlayerDown
    }

    /// <summary>
    /// The enemy starts roaming when he awakes
    /// </summary>
    private void Awake()
    {
        state = State.Roaming;
    }

    /// <summary>
    /// Called every frame. Limit attack frequency.
    /// </summary>
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

    /// <summary>
    /// Set a random timeframe for how long before the next decision is made 
    /// </summary>
    private void Start()
    {
        decisionTimeCount = UnityEngine.Random.Range(decisionTime.x, decisionTime.y);
        ChooseMoveDirection();
    }

    /// <summary>
    /// Selects a random movement direction.
    /// </summary>
    private void ChooseMoveDirection()
    {
        currentMoveDirection = Mathf.FloorToInt(UnityEngine.Random.Range(0, moveDirections.Length));
    }

    /// <inheritdoc cref="NetworkBehaviour.OnNetworkSpawn"/>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        players = GameObject.FindGameObjectsWithTag("Player").ToList();
    }

    /// <summary>
    /// Event handler for player knockdown event.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPlayerKnockdown(object sender, PlayerHealth.OnPlayerKnockdownEventArgs e)
    {
        if (players.Any())
        {
            if (indexOfChasedPlayer >= players.Count) return;

            if (players[indexOfChasedPlayer].transform == target.transform)
            {
                players.RemoveAt(indexOfChasedPlayer);
                target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown -= OnPlayerKnockdown;
            }
        }

        StopAnimationClientRpc();
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

                    SearchForTarget();

                    if (decisionTimeCount > 0) decisionTimeCount -= Time.deltaTime;
                    else
                    {
                        decisionTimeCount = UnityEngine.Random.Range(decisionTime.x, decisionTime.y);
                        ChooseMoveDirection();
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

    /// <summary>
    /// Take a break after defeating a player, resume roaming afterwards
    /// </summary>
    private IEnumerator BreakBeforeRoaming()
    {
        yield return new WaitForSeconds(3f);
        state = State.Roaming;
    }

    /// <summary>
    /// Searches for a target within the chasing range.
    /// </summary>
    private void SearchForTarget()
    {
        if (players.Any())
        {
            foreach (GameObject player in players)
            {
                if (player == null) return;
                if (Vector2.Distance(player.transform.position, transform.position) < stopChasingDistance)
                {
                    indexOfChasedPlayer = players.IndexOf(player);
                    state = State.ChaseTarget;
                    target = player.transform;
                    target.GetComponentInChildren<PlayerHealth>().OnPlayerKnockdown += OnPlayerKnockdown;
                }
            }
        }
    }

    /// <summary>
    /// Chase the target player.
    /// </summary>
    private void ChaseTarget()
    {
        if (players.Any())
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

    /// <summary>
    /// React to the circle collider to pick new targets within range.
    /// </summary>
    /// <param name="collision">The collision information.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer) return;
        if (players.Any())
        {
            if (collision.GetComponentInParent<PlayerBehaviour>() && !collision.isTrigger)
            {
                target = collision.transform;
            }
        }
    }

    private void StopAnimation()
    {
        moveDirection = Vector2.zero;
        animator.SetFloat(SPEED, 0);
    }

    /// <summary>
    /// Tell clients to stop animating the enemy when he is taking a break
    /// </summary>
    [ClientRpc]
    private void StopAnimationClientRpc()
    {
        StopAnimation();
    }

    /// <summary>
    /// Use the new sword with a more powerful attack
    /// </summary>
    private void Attack()
    {
        animator.SetTrigger(STEELATTACK);
    }
}
