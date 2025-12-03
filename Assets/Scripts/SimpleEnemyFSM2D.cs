using UnityEngine;

public class SimpleEnemyFSM2D : MonoBehaviour
{
    private enum EnemyState
    {
        Patrolling,
        Chasing
    }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 2f;

    [Header("Patrol")]
    [SerializeField] private float moveDirX = -1f;
    [SerializeField] private float _offset;
    [SerializeField] private float _rayDistance;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Chase Logic")]
    [SerializeField] private float stopDistance = 0.1f;

    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Vida inicial
        currentHealth = maxHealth;

        // Buscar player si no está asignado
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (player == null) return;

        // === COMPORTAMIENTO ===
        switch (currentState)
        {
            case EnemyState.Patrolling:
                DoPatrol();
                break;

            case EnemyState.Chasing:
                DoChase();
                break;
        }
    }

    private void FixedUpdate()
    {
        float speed = (currentState == EnemyState.Chasing) ? chaseSpeed : patrolSpeed;
        rb.linearVelocity = new Vector2(moveDirX * speed, rb.linearVelocity.y);
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
            UnityEditor.Handles.DrawWireDisc(collider.transform.position, Vector3.back, collider.radius * transform.localScale.z);
#endif
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            currentState = EnemyState.Chasing;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            currentState = EnemyState.Patrolling;
    }

    // ============================================================
    // ESTADOS
    // ============================================================

    private void DoPatrol()
    {
        if (spriteRenderer) spriteRenderer.color = Color.yellow;

        RaycastHit2D ray = Physics2D.Raycast(
            transform.position - new Vector3(0, _offset, 0),
            new Vector2(moveDirX, 0),
            _rayDistance
        );

        Debug.DrawLine(
            transform.position - new Vector3(0, _offset, 0),
            transform.position - new Vector3(0, _offset, 0) + new Vector3(moveDirX, 0, 0) * _rayDistance
        );

        if (ray && ray.collider.CompareTag("Floor"))
        {
            moveDirX *= -1; // cambiar dirección
        }
    }

    private void DoChase()
    {
        if (spriteRenderer) spriteRenderer.color = Color.red;

        float dx = player.position.x - transform.position.x;

        if (Mathf.Abs(dx) < stopDistance)
            moveDirX = 0;
        else
            moveDirX = Mathf.Sign(dx);
    }

    // ============================================================
    // DAÑO Y MUERTE
    // ============================================================

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;

        // Feedback visual instantáneo al recibir daño
        if (spriteRenderer) spriteRenderer.color = Color.white;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
