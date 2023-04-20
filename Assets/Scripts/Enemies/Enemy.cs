using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Transform target;
    private const float SPEED_VALUE = 2f;
    private const string HORIZONTAL = "Horizontal";
    private const string VERTICAL = "Vertical";
    private const string SPEED = "Speed";
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void FixedUpdate()
    {
        if (target)
        {
            Vector2 moveDirection = (target.transform.position - transform.position).normalized;
            transform.Translate(SPEED_VALUE * Time.fixedDeltaTime * moveDirection);
            //transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);

            animator.SetFloat(HORIZONTAL, moveDirection.x);
            animator.SetFloat(VERTICAL, moveDirection.y);
            animator.SetFloat(SPEED, moveDirection.sqrMagnitude);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<PlayerBehaviour>())
        {
            target = collision.transform;
        }
    }
}
