using UnityEngine;

public class SimpleEnemyFSM2D : MonoBehaviour
{
    // Estados posibles del enemigo:
    // - Patrolling: patrulla entre dos puntos.
    // - Chasing: persigue al jugador cuando se acerca.
    private enum EnemyState
    {
        Patrolling,
        Chasing
    }

    [Header("References")]
    [SerializeField] private Transform player;          // Referencia al jugador a seguir
    [SerializeField] private Transform patrolPointA;    // Punto A de patrulla
    [SerializeField] private Transform patrolPointB;    // Punto B de patrulla

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;    // Velocidad mientras patrulla
    [SerializeField] private float chaseSpeed = 3.5f;   // Velocidad mientras persigue

    [Header("Detection")]
    [SerializeField] private float detectionRange = 4f; // Distancia a la que empieza a perseguir
    [SerializeField] private float loseRange = 6f;      // Distancia a la que deja de perseguir

    // Estado actual del enemigo (por defecto, patrulla)
    private EnemyState currentState = EnemyState.Patrolling;

    // Punto de patrulla al que se dirige actualmente (A o B)
    private Transform currentPatrolTarget;

    // Referencias a componentes
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Dirección de movimiento en X: -1 (izquierda), 0 (quieto), 1 (derecha)
    private float moveDirX = 0f;

    private void Awake()
    {
        // Guardamos las referencias a SpriteRenderer y Rigidbody2D
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Empezamos patrullando hacia el punto A
        currentPatrolTarget = patrolPointA;
    }

    private void Update()
    {
        if (player == null) return;

        // Distancia actual al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ============================
        // SWITCH 1 — COMPORTAMIENTO
        // (qué hace el enemigo según su estado actual)
        // ============================
        switch (currentState)
        {
            case EnemyState.Patrolling:
                DoPatrol();  // Patrullar entre A y B
                break;

            case EnemyState.Chasing:
                DoChase();   // Perseguir al jugador
                break;
        }

        // ============================
        // SWITCH 2 — TRANSICIONES
        // (cuándo cambia de patrullar a perseguir y viceversa)
        // ============================
        switch (currentState)
        {
            case EnemyState.Patrolling:
                // Si el jugador entra en el rango de detección → pasamos a Chasing
                if (distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.Chasing;
                }
                break;

            case EnemyState.Chasing:
                // Si el jugador se aleja demasiado → volvemos a Patrullar
                if (distanceToPlayer >= loseRange)
                {
                    currentState = EnemyState.Patrolling;
                }
                break;
        }

        // Flip visual según la dirección de movimiento (opcional)
        if (spriteRenderer != null && Mathf.Abs(moveDirX) > 0.01f)
        {
            // Si se mueve hacia la izquierda, flipX = true
            spriteRenderer.flipX = moveDirX < 0f;
        }
    }

    private void FixedUpdate()
    {
        // Elegimos velocidad según el estado:
        // - Si está persiguiendo → chaseSpeed
        // - Si está patrullando → patrolSpeed
        float currentSpeed = (currentState == EnemyState.Chasing) ? chaseSpeed : patrolSpeed;

        // Aplicamos la velocidad horizontal. La Y la controla la gravedad y otras fuerzas.
        rb.linearVelocity = new Vector2(moveDirX * currentSpeed, rb.linearVelocity.y);
    }

    // --------------------------------------------------------------------
    // COMPORTAMIENTO POR ESTADO
    // --------------------------------------------------------------------

    // Estado: PATRULLA
    private void DoPatrol()
    {
        // Color amarillo para visualizar que está patrullando
        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;

        if (currentPatrolTarget == null) return;

        float targetX = currentPatrolTarget.position.x;
        float dx = targetX - transform.position.x;

        // Si estamos muy cerca del punto de patrulla actual,
        // cambiamos al otro (de A a B, o de B a A)
        if (Mathf.Abs(dx) < 0.05f)
        {
            currentPatrolTarget = (currentPatrolTarget == patrolPointA) ? patrolPointB : patrolPointA;
            dx = currentPatrolTarget.position.x - transform.position.x;
        }

        // Nos movemos hacia el punto de patrulla:
        // -1 si está a la izquierda, 1 si está a la derecha
        moveDirX = Mathf.Sign(dx);
    }

    // Estado: PERSEGUIR AL JUGADOR
    private void DoChase()
    {
        // Color rojo para indicar modo persecución
        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        float dx = player.position.x - transform.position.x;

        // Si estamos casi alineados con el jugador en X, nos paramos
        if (Mathf.Abs(dx) < 0.01f)
        {
            moveDirX = 0f;
        }
        else
        {
            // Si el jugador está a la derecha → nos movemos a la derecha
            // Si está a la izquierda → nos movemos a la izquierda
            moveDirX = Mathf.Sign(dx);
        }
    }

    // --------------------------------------------------------------------
    // MUERTE DEL ENEMIGO
    // --------------------------------------------------------------------

    public void Die()
    {
        // Aquí se podría añadir:
        // - Animación de muerte
        // - Sonido
        // - Partículas
        // De momento simplemente destruimos el GameObject del enemigo.
        Destroy(gameObject);
    }
}
