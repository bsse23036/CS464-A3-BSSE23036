using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Launch Physics")]
    [Tooltip("Magnitude of the impulse force applied to the player.")]
    [SerializeField] private float launchForce = 14f;

    [Tooltip("Local direction to launch the player (0,1,0 for straight up).")]
    [SerializeField] private Vector3 launchDirection = Vector3.up;

    [Tooltip("Zeroes out Y velocity before launch so falling momentum doesn't dampen the jump.")]
    [SerializeField] private bool resetYVelocity = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Reset Y velocity to eliminate falling momentum interference
                if (resetYVelocity)
                {
                    playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);
                }

                // Transform local launch direction to world space and apply ForceMode.Impulse
                Vector3 worldDirection = transform.TransformDirection(launchDirection.normalized);
                playerRb.AddForce(worldDirection * launchForce, ForceMode.Impulse);

                Debug.Log($"JumpPad launched player with force: {launchForce}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visual arrow in Scene View showing launch direction
        Gizmos.color = Color.cyan;
        Vector3 worldDir = transform.TransformDirection(launchDirection.normalized);
        Gizmos.DrawRay(transform.position, worldDir * 2f);
    }
}