using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackDelay = 0.35f;
    private Transform _player;
    [SerializeField] private Transform PunchHitbox;
    [SerializeField] private float pushDuration = 0.6f;
    [SerializeField] private float pushForce = 180f;
    [SerializeField] private float playerTrackingDistance = 10f;
    [SerializeField] private float paralysisTimePerHit = 1f;
    [SerializeField] private float TimeBetweenAttacks = 1.5f;
    [SerializeField] private Transform HipsPosition = null;

    private Animator _animator;

    private bool canAttack = true;
    private bool canMove = true;
    private bool isKnockedOut = false;
    private AudioSource _audioSource = null;

    private Collider _mainCollider = null;
    private Collider[] _ragdollColliders = null;
    private Rigidbody[] _ragdollRigidbodies = null;

    private void Start()
    {
        _mainCollider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        _player = GameObject.FindGameObjectWithTag("PlayerPos").transform;
        _audioSource = GetComponent<AudioSource>();
        PunchHitbox.gameObject.SetActive(true);
        PunchHitbox.gameObject.SetActive(false);
        RagdollParts();
        RagdollOff();
        StartCoroutine(Attack());
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        if (distanceToPlayer <= playerTrackingDistance && canMove)
        {
            if (distanceToPlayer <= attackRange && canAttack)
            {
                StartCoroutine(Attack());
            }
            else
            {
                FollowPlayer();
            }
        }
        else
        {
            _animator.SetFloat("VelX", 0);
            _animator.SetFloat("VelY", 0);
        }
    }

    private void FollowPlayer()
    {
        Vector3 direction = (_player.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        lookRotation.x = 0;
        lookRotation.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        _animator.SetFloat("VelX", direction.x);
        _animator.SetFloat("VelY", direction.z);
    }

    private IEnumerator Attack()
    {
        Debug.Log("Inicio de ataque");
        canAttack = false;
        yield return new WaitForSeconds(0.3f);
        if (isKnockedOut)
        {
            canAttack = true;
            yield break;
        }
        Debug.Log("Inicio de animación");
        _animator.SetTrigger("Punch");
        yield return new WaitForSeconds(attackDelay);

        Debug.Log("Hitbox activada");
        PunchHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Hitbox desactivada");
        PunchHitbox.gameObject.SetActive(false);

        yield return new WaitForSeconds(TimeBetweenAttacks);
        canAttack = true;
    }

    public void HandleHit(Collider playerCollider)
    {
        if (playerCollider.CompareTag("Player") && !isKnockedOut)
        {
            PlayerMove playerMove = playerCollider.GetComponent<PlayerMove>();
            _audioSource.Play();

            if (playerMove != null)
            {
                Vector3 pushDirection = (playerCollider.transform.position - transform.position).normalized;
                if (playerMove.IsRagdollActive())
                {
                    playerMove.ApplyForceToRagdoll(pushDirection * pushForce);
                }
                else
                {
                    Rigidbody playerRb = playerCollider.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        StartCoroutine(PushPlayer(playerRb, pushDirection, playerMove));
                    }
                }
            }
        }
    }

    private IEnumerator PushPlayer(Rigidbody playerRb, Vector3 pushDirection, PlayerMove playerMove)
    {
        playerMove.KnockedOut(paralysisTimePerHit);
        if (playerMove.IsRagdollActive())
        {
            playerMove.ApplyForceToRagdoll(pushDirection * pushForce);
        }
        else
        {
            for (float t = 0; t < pushDuration; t += Time.deltaTime)
            {
                playerRb.AddForce(pushDirection * (pushForce * Time.deltaTime / pushDuration), ForceMode.Impulse);
                yield return null;
            }
        }
    }

    public void KnockedOut(float duration)
    {
        StopCoroutine(Attack());
        StartCoroutine(Paralyze(duration));
    }

    private IEnumerator Paralyze(float duration)
    {
        canAttack = false;
        canMove = false;
        isKnockedOut = true;
        RagdollOn();
        _animator.SetFloat("VelX", 0);
        _animator.SetFloat("VelY", 0);
        yield return new WaitForSeconds(duration);
        RagdollOff();
        canAttack = true;
        canMove = true;
        isKnockedOut = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerTrackingDistance);
    }

    private void RagdollOn()
    {
        _animator.enabled = false;

        foreach (Collider ragdollCollider in _ragdollColliders)
        {
            ragdollCollider.enabled = true;
        }
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            ragdollRigidbody.isKinematic = false;
        }

        _mainCollider.enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void RagdollOff()
    {
        //Rigidbody ragdollMainBody = GetMainRagdollBody();
        Transform ragdollMainBody = HipsPosition;

        if (ragdollMainBody != null)
        {
            transform.position = ragdollMainBody.position;
            transform.rotation = Quaternion.identity;
        }

        foreach (Collider ragdollCollider in _ragdollColliders)
        {
            ragdollCollider.enabled = false;
        }
        foreach (Rigidbody ragdollRigidbody in _ragdollRigidbodies)
        {
            ragdollRigidbody.isKinematic = true;
        }

        _mainCollider.enabled = true;
        _animator.enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
    }

    private void RagdollParts()
    {
        _ragdollColliders = GetComponentsInChildren<Collider>();
        _ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    public bool IsRagdollActive()
    {
        return !canMove;
    }

    public void ApplyForceToRagdoll(Vector3 force)
    {
        foreach (var ragdollRigidbody in _ragdollRigidbodies)
        {
            if (ragdollRigidbody != null && ragdollRigidbody.gameObject.activeSelf)
            {
                ragdollRigidbody.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    private Rigidbody GetMainRagdollBody()
    {
        foreach (Rigidbody rigidbody in _ragdollRigidbodies)
        {
            if (rigidbody.name.ToLower().Contains("hips") || rigidbody.name.ToLower().Contains("spine"))
            {
                return rigidbody;
            }
        }
        return null;
    }
}