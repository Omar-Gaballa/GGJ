using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damageAmount = 20f;

    [Header("UI")]
    public Slider healthSlider;
    public GameObject deathCanvas;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;

        if (deathCanvas != null)
            deathCanvas.SetActive(false);
    }

    // void Update()
    // {
    //     if (isDead) return;
    //
    //     // SPACE key takes damage
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         TakeDamage(damageAmount);
    //     }
    // }

    void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log("Player Died");

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        // Optional extras:
        // Time.timeScale = 0f; // freeze game
        // GetComponent<PlayerMovement>().enabled = false;
    }
}