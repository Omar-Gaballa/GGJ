using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;          // Player transform
    public GameObject playerObject;   // Player GameObject (optional convenience)

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject deathCanvas;

    private bool isDead = false;

    void Awake()
    {
        // Auto-assign player if not set
        if (player == null)
            player = transform;

        if (playerObject == null)
            playerObject = player.gameObject;
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogWarning("PlayerHealth: Health Slider is NOT assigned.");
        }

        if (deathCanvas != null)
            deathCanvas.SetActive(false);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        Debug.Log($"Player {playerObject.name} took {damage} damage. HP = {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Player Died");

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        // Optional extras:
        // Time.timeScale = 0f;
        // GetComponent<PlayerMovement>().enabled = false;
    }
}