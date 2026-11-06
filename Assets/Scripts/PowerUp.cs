using UnityEngine;

public enum PowerUpType { Shield, Invincibility }

public class PowerUp : MonoBehaviour
{
    [Header("Power-Up Configuration")]
    [SerializeField] private PowerUpType powerUpType = PowerUpType.Shield;
    [SerializeField] private float invincibilityDuration = 3.0f;

    [Header("Animation Settings")]
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 0.2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Continuous rotation & floating motion
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
            {
                ApplyPowerUp(playerHealth);
                Destroy(gameObject);
            }
        }
    }

    private void ApplyPowerUp(Health health)
    {
        switch (powerUpType)
        {
            case PowerUpType.Shield:
                // Activates the damage-absorption shield on Health.cs
                health.shielded = true;
                Debug.Log("Shield acquired!");
                break;

            case PowerUpType.Invincibility:
                // Restores max health and grants temporary i-frames
                health.Restore(invincibilityDuration);
                Debug.Log($"Invincibility active for {invincibilityDuration} seconds!");
                break;
        }

        // Refresh UI state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }
    }
}