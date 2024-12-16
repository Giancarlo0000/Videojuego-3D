using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float runSpeed = 7;
    [SerializeField] private float jumpForce = 6.5f;
    [SerializeField] private float pushForce = 200f;
    [SerializeField] private float pushDuration = 0.6f;
    [SerializeField] private float attackDelay = 0.35f;
    [SerializeField] private float paralysisTimePerHit = 1f;
    [SerializeField] private Transform punchHitbox;
    [SerializeField] private Transform HipsPosition = null;

    [SerializeField] private AudioClip HitSound = null;
    [SerializeField] private AudioClip DefeatSound = null;
    [SerializeField] private AudioClip JumpSound = null;
    [SerializeField] private ParticleSystem HitParticles = null;

    private float x, y;
    private bool isGrounded;
    private bool canAttack = true;
    private bool canMove = true;
    private bool isKnockedOut = false;
    private AudioSource _audioSource = null;
    private AudioManager _audioManager = null;
    //private bool _hasDefeated = false;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    private Rigidbody rb;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private Collider _mainCollider = null;
    private Animator _animator = null;

    private Collider[] _ragdollColliders = null;
    private Rigidbody[] _ragdollRigidbodies = null;

    private void Start()
    {
        _mainCollider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        punchHitbox.gameObject.SetActive(false);
        _audioManager = FindObjectOfType<AudioManager>();
        RagdollParts();
        RagdollOff();
    }

    void Update()
    {
        RotateCamera();

        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        if (canMove)
        {
            MovePlayer();
        }

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask);
        _animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(Attack());
        }
    }

    private void RotateCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -45f, 45f);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void MovePlayer()
    {
        Vector3 move = transform.right * x + transform.forward * y;
        rb.MovePosition(rb.position + move * runSpeed * Time.deltaTime);
        _animator.SetFloat("VelX", x);
        _animator.SetFloat("VelY", y);
    }

    private void Jump()
    {
        _audioSource.clip = JumpSound;
        _audioSource.Play();
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _animator.SetTrigger("Jump");
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        _animator.SetTrigger("Punch");

        yield return new WaitForSeconds(attackDelay);

        punchHitbox.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        punchHitbox.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);
        canAttack = true;
    }

    public void HandleHit(Collider enemy)
    {
        if (enemy.CompareTag("Enemy") && !isKnockedOut)
        {
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

            _audioSource.clip = HitSound;
            _audioSource.Play();
            HitParticles.Play();
            StartCoroutine(StopHitParticles());

            if (enemyAI != null)
            {
                Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;
                if (enemyAI.IsRagdollActive())
                {
                    enemyAI.ApplyForceToRagdoll(pushDirection * pushForce);
                }
                else
                {
                    Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        StartCoroutine(PushEnemy(enemyRb, pushDirection, enemyAI));
                    }
                }
            }
        }
    }
    private IEnumerator StopHitParticles()
    {
        yield return new WaitForSeconds(HitParticles.main.duration);
        HitParticles.Stop();
    }

    private IEnumerator PushEnemy(Rigidbody enemyRb, Vector3 pushDirection, EnemyAI enemyAI)
    {
        enemyAI.KnockedOut(paralysisTimePerHit);
        if (enemyAI.IsRagdollActive())
        {
            enemyAI.ApplyForceToRagdoll(pushDirection * pushForce);
        }
        else
        {
            for (float t = 0; t < pushDuration; t += Time.deltaTime)
            {
                enemyRb.AddForce(pushDirection * (pushForce * Time.deltaTime / pushDuration), ForceMode.Impulse);
                yield return null;
            }
        }
        
    }

    public void KnockedOut(float duration)
    {
        StartCoroutine(Paralyze(duration));
    }

    private IEnumerator Paralyze(float duration)
    {
        canAttack = false;
        canMove = false;
        isKnockedOut = true;
        RagdollOn();
        yield return new WaitForSeconds(duration);
        RagdollOff();
        canMove = true;
        canAttack = true;
        isKnockedOut = false;
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

        _mainCollider.enabled = false;
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
        foreach (Rigidbody rb in _ragdollRigidbodies)
        {
            if (rb.name.ToLower().Contains("hips") || rb.name.ToLower().Contains("spine"))
            {
                return rb;
            }
        }
        return null;
    }

    public void PlayerDefeated()
    {
        playerCamera.transform.parent = null;
        //_hasDefeated = true;
        _audioManager.StopMusic();
        _audioSource.clip = DefeatSound;
        _audioSource.Play();
    }
}