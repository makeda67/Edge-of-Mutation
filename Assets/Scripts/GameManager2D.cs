using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager2D : MonoBehaviour
{
    public static GameManager2D Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private PlayerStateMachine2D player;
    [SerializeField] private Rigidbody2D playerRb;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;

    [Header("UI")]
    [SerializeField] private TMP_Text coinsText;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Mantener entre escenas si quieres (opcional)
        // DontDestroyOnLoad(gameObject);

        // Buscar Rigidbody si no está asignado
        if (player != null && playerRb == null)
            playerRb = player.GetComponent<Rigidbody2D>();

        UpdateCoinsUI(0);
    }

    // -----------------------------------------------------------
    // MONEDAS
    // -----------------------------------------------------------
    public void UpdateCoinsUI(int amount)
    {
        if (coinsText != null)
            coinsText.text = amount.ToString();
    }

    // -----------------------------------------------------------
    // MUERTE Y RESPAWN
    // -----------------------------------------------------------
    public void KillPlayer()
    {
        // Reiniciar escena (como ya hacías en el script del jugador)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Si quisieras respawn sin recargar escena:
    public void RespawnPlayer()
    {
        if (player == null || respawnPoint == null) return;

        player.transform.position = respawnPoint.position;
        playerRb.linearVelocity = Vector2.zero;
    }
}
