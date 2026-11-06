using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    public bool shielded = false;
    [SerializeField] private float iFrameDuration = 1.0f;
    [SerializeField] private float defaultKnockbackForce = 10f;

    private float invincibleUntil;
    private Rigidbody rb;

    public int Current => currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
    }

    // Overload allowing default damage without knockback position if needed
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position - transform.forward, defaultKnockbackForce);
    }

    // Main damage method accepting source position for knockback calculations
    public void TakeDamage(int damage, Vector3 damageSourcePosition, float force = 10f)
    {
        if (Time.time < invincibleUntil) return;

        if (shielded)
        {
            shielded = false;
            invincibleUntil = Time.time + iFrameDuration;
            Debug.Log("Shield absorbed the damage!");
            ApplyKnockback(damageSourcePosition, force);
            return;
        }

        currentHealth -= damage;
        invincibleUntil = Time.time + iFrameDuration;

        ApplyKnockback(damageSourcePosition, force);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }

        if (currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDied();
            }
        }
    }

    private void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        if (rb == null) return;

        // Calculate direction pointing away from damage source
        Vector3 knockbackDir = (transform.position - sourcePosition);
        knockbackDir.y = 0; // Flatten XZ direction vector
        knockbackDir = knockbackDir.normalized;

        // Add slight upward lift (0.4) to break ground friction during impulse
        knockbackDir.y = 0.4f;
        knockbackDir.Normalize();

        // Zero out current velocity so falling/moving momentum doesn't dampen impulse force
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(knockbackDir * force, ForceMode.Impulse);
    }

    public void Restore(float invincibilityTime)
    {
        currentHealth = maxHealth;
        invincibleUntil = Time.time + invincibilityTime;
    }
}