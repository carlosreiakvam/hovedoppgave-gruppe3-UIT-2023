using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Transform target;
    private readonly float speed = 1f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void FixedUpdate()
    {
        if (target)
        {
            Vector2 moveDirection = (target.transform.position - transform.position).normalized;
            transform.Translate(speed * Time.fixedDeltaTime * moveDirection);
            //transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);

            animator.SetFloat("Horizontal", moveDirection.x);
            animator.SetFloat("Vertical", moveDirection.y);
            animator.SetFloat("Speed", moveDirection.sqrMagnitude);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
            target = collision.transform;
    }
}
