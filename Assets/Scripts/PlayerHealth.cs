using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    private PlayerShield shield;

    void Start()
    {
        shield = GetComponent<PlayerShield>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (shield != null)
        {
            damage = shield.Absorb(damage);
        }

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            gameObject.SetActive(false);
        }
    }
}
