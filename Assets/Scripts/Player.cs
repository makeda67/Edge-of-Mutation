using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerStateMachine2D : MonoBehaviour
{
    private enum State
    {
        Idle,
        Walking,
        Jumping,
        Knockback,
        Attacking   
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float jumpForce = 4f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Attack")]
    [SerializeField] private float attackDuration = 0.25f; 

    [Header("Coins")]
    [SerializeField] private TMP_Text textCoins;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackTime = 0.35f;

    private int coins;

    private State currentState = State.Idle;

    private Rigidbody2D rb;
    private Animator animator;

    private float inputX;
    private bool jumpPressed;
    private bool attackPressed;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (GameManager2D.Instance != null)
        {
            GameManager2D.Instance.UpdateCoinsUI(coins);
        }
    }

    private void Update()
    {
        ReadInput();

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // --- COMPORTAMIENTO ---
        switch (currentState)
        {
            case State.Idle: DoIdle(); break;
            case State.Walking: DoWalking(); break;
            case State.Jumping: DoJumping(); break;
            case State.Knockback: DoKnockback(); break;
            case State.Attacking: DoAttacking(); break; // ← NUEVO
        }

        // --- TRANSICIONES ---
        switch (currentState)
        {
            case State.Idle:
                if (attackPressed && isGrounded)
                    StartCoroutine(AttackRoutine());

                if (Mathf.Abs(inputX) > 0.1f && isGrounded)
                    currentState = State.Walking;

                if (jumpPressed && isGrounded)
                {
                    Jump();
                    currentState = State.Jumping;
                }
                break;

            case State.Walking:
                if (attackPressed && isGrounded)
                    StartCoroutine(AttackRoutine());

                if (Mathf.Abs(inputX) <= 0.1f)
                    currentState = State.Idle;

                if (jumpPressed && isGrounded)
                {
                    Jump();
                    currentState = State.Jumping;
                }
                break;

            case State.Jumping:
                if (isGrounded && rb.linearVelocity.y <= 0.01f)
                    currentState = Mathf.Abs(inputX) > 0.1f ? State.Walking : State.Idle;
                break;

            case State.Knockback:
                break;

            case State.Attacking:
                break; 
        }

        // Animaciones
        animator.SetFloat("Speed", Mathf.Abs(inputX));
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    private void FixedUpdate()
    {
        if (currentState == State.Knockback || currentState == State.Attacking)
            return; // NO moverse mientras golpea

        rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);

        if (inputX != 0)
            transform.localScale = new Vector3(Mathf.Sign(inputX), 1f, 1f);
    }

    private void ReadInput()
    {
        inputX = 0f;
        jumpPressed = false;
        attackPressed = false;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            inputX = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            inputX = 1f;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            jumpPressed = true;

        if (Keyboard.current.kKey.wasPressedThisFrame)
            attackPressed = true;   
    }

    private void DoIdle() { }
    private void DoWalking() { }
    private void DoJumping() { }
    private void DoKnockback() { }
    private void DoAttacking() { } 

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // --- ATAQUE ---
    private System.Collections.IEnumerator AttackRoutine()
    {
        currentState = State.Attacking;
        animator.SetTrigger("Attack 2");

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Quieto

        yield return new WaitForSeconds(attackDuration);

        currentState = isGrounded ?
            (Mathf.Abs(inputX) > 0.1f ? State.Walking : State.Idle)
            : State.Jumping;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            coins++;
            if (GameManager2D.Instance != null)
                GameManager2D.Instance.UpdateCoinsUI(coins);

            Destroy(collision.gameObject);
        }

        if (collision.CompareTag("Spikes"))
        {
            if (GameManager2D.Instance != null)
                GameManager2D.Instance.KillPlayer();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (collision.CompareTag("Barrel"))
        {
            StartCoroutine(KnockbackRoutine(collision));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (GameManager2D.Instance != null)
                GameManager2D.Instance.KillPlayer();
            else
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private System.Collections.IEnumerator KnockbackRoutine(Collider2D collision)
    {
        currentState = State.Knockback;

        Vector2 dir = (rb.position - (Vector2)collision.transform.position).normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);

        foreach (var col in collision.GetComponents<BoxCollider2D>())
            col.enabled = false;

        collision.GetComponent<Animator>().enabled = true;
        Destroy(collision.gameObject, 0.5f);

        yield return new WaitForSeconds(knockbackTime);

        currentState = State.Idle;
    }
}
