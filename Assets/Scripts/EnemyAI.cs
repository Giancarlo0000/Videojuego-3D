using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackDelay = 0.35f;
    [SerializeField] private Transform player;
    [SerializeField] private Transform punchTransform;
    [SerializeField] private float pushDuration = 0.6f;
    [SerializeField] private float pushForce = 180f;
    [SerializeField] private float playerTrackingDistance = 10f;
    [SerializeField] private float paralysisTimePerHit = 1f;

    [SerializeField] private Animator animator;

    private bool canAttack = true;
    private bool canMove = true;
    private bool isKnockedOut = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && canAttack)
        {
            StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= playerTrackingDistance && canMove)
        {
            FollowPlayer();
        }
        else
        {
            animator.SetFloat("VelX", 0);
            animator.SetFloat("VelY", 0);
        }
    }

    private void FollowPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation.x = 0;
        lookRotation.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        animator.SetFloat("VelX", direction.x);
        animator.SetFloat("VelY", direction.z);
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.3f);
        if (!isKnockedOut)
        {
            animator.SetTrigger("Punch");
        }
        yield return new WaitForSeconds(attackDelay);

        Collider[] players = Physics.OverlapSphere(punchTransform.position, attackRange);

        foreach (Collider playerCollider in players)
        {
            if (playerCollider.CompareTag("Player") && !isKnockedOut)
            {
                Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
                Animator enemyAnimator = playerCollider.GetComponent<Animator>();
                PlayerMove playerMove = playerCollider.GetComponent<PlayerMove>();
                if (playerRb != null)
                {
                    enemyAnimator.SetTrigger("KnockOut");
                    Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;

                    for (float t = 0; t < pushDuration; t += Time.deltaTime)
                    {
                        playerRb.AddForce(pushDirection * (pushForce * Time.deltaTime / pushDuration), ForceMode.Impulse);
                        yield return null;
                    }
                    playerMove.KnockedOut(paralysisTimePerHit);
                }
            }
        }
        yield return new WaitForSeconds(0.6f);
        canAttack = true;
    }

    public void KnockedOut(float duration)
    {
        StartCoroutine(Paralyze(duration));
        StopCoroutine(Attack());
    }

    private IEnumerator Paralyze(float duration)
    {
        canAttack = false;
        canMove = false;
        isKnockedOut = true;
        animator.SetFloat("VelX", 0);
        animator.SetFloat("VelY", 0);
        yield return new WaitForSeconds(duration);
        canAttack = true;
        canMove = true;
        isKnockedOut = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(punchTransform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerTrackingDistance);
    }
}