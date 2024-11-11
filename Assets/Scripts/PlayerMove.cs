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
    [SerializeField] private Animator animator;

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
    private bool _hasDefeated = false;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    private Rigidbody rb;

    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    private float verticalRotation = 0f;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        punchHitbox.gameObject.SetActive(false);
        _audioManager = FindObjectOfType<AudioManager>();
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
        animator.SetBool("IsGrounded", isGrounded);

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            StartCoroutine(Attack());
        }

        if (transform.position.y < -15f && !_hasDefeated)
        {
            _hasDefeated = true;
            _audioManager.StopMusic();
            _audioSource.clip = DefeatSound;
            _audioSource.Play();
            StartCoroutine(ResetScene());
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
        animator.SetFloat("VelX", x);
        animator.SetFloat("VelY", y);
    }

    private void Jump()
    {
        _audioSource.clip = JumpSound;
        _audioSource.Play();
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        animator.SetTrigger("Punch");

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
            Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
            Animator enemyAnimator = enemy.GetComponent<Animator>();
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

            _audioSource.clip = HitSound;
            _audioSource.Play();

            HitParticles.Play();
            StartCoroutine(StopHitParticles());

            if (enemyRb != null)
            {
                enemyAnimator.SetTrigger("KnockOut");

                Vector3 pushDirection = (enemy.transform.position - transform.position).normalized;

                StartCoroutine(PushEnemy(enemyRb, pushDirection, enemyAI));
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
        for (float t = 0; t < pushDuration; t += Time.deltaTime)
        {
            enemyRb.AddForce(pushDirection * (pushForce * Time.deltaTime / pushDuration), ForceMode.Impulse);
            yield return null;
        }
        enemyAI.KnockedOut(paralysisTimePerHit);
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
        yield return new WaitForSeconds(duration);
        canMove = true;
        canAttack = true;
        isKnockedOut = false;
    }

    private IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(_audioSource.clip.length);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
