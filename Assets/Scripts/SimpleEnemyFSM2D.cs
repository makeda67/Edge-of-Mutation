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
    [SerializeField] private float stopDistance = 0.1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float loseRange = 6f;

    [SerializeField] private EnemyState currentState = EnemyState.Patrolling;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [SerializeField] private float moveDirX = -1f;
    [SerializeField] private float _offset;
    [SerializeField] private float _rayDistance;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Si falta el jugador, intenta buscarlo por tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

    }

    private void Update()
    {

        
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);



        // --- COMPORTAMIENTO ---
        switch (currentState)
        {
            case EnemyState.Patrolling:
                DoPatrol();
                break;

            case EnemyState.Chasing:
                DoChase();
                break;
        }

        

        

        // Voltear sprite según movimiento
       // if (Mathf.Abs(moveDirX) > 0.05f)
          //  spriteRenderer.flipX = moveDirX < 0;
    }
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = Color.yellow;
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
            UnityEditor.Handles.DrawWireDisc(collider.transform.position, Vector3.back, collider.radius * transform.localScale.z);

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            // if (currentState == EnemyState.Patrolling)
            //{
                currentState = EnemyState.Chasing;
            //}

        }

    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
           // if (currentState == EnemyState.Chasing)
           // {

                currentState = EnemyState.Patrolling;
            //}
        }

    }




    private void FixedUpdate()
    {
        float speed = (currentState == EnemyState.Chasing) ? chaseSpeed : patrolSpeed;
        rb.linearVelocity = new Vector2(moveDirX * speed, rb.linearVelocity.y);
        Debug.Log(speed);
        Debug.Log(moveDirX);
        Debug.Log(rb.linearVelocity);
    }

    // =========== ESTADOS ===========

    private void DoPatrol()
    {
        if (spriteRenderer) spriteRenderer.color = Color.yellow;
        RaycastHit2D ray = Physics2D.Raycast(transform.position - new Vector3(0, _offset, 0), new Vector3(moveDirX, 0, 0), _rayDistance);
        Debug.DrawLine(transform.position - new Vector3(0, _offset, 0), transform.position - new Vector3(0, _offset, 0) + new Vector3(moveDirX, 0, 0) * _rayDistance);

        if (ray)
        {
            Debug.Log(ray.collider.gameObject.name);

            if (ray.collider.gameObject.tag.Equals("Floor"))
            {
                moveDirX *= -1;

            }

        }
        // Cambiar de objetivo cuando se alcanza el punto



    }

    private void DoChase()
    {
        if (spriteRenderer) spriteRenderer.color = Color.red;

        float dx = player.position.x - transform.position.x;

        if (Mathf.Abs(dx) < 0.05f)
            moveDirX = 0;
        else
            moveDirX = Mathf.Sign(dx);
    }

    // =========== MUERTE ===========

    public void Die()
    {
        Destroy(gameObject);
    }
}
