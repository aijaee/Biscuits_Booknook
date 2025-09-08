using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Range(0f, 1f)]
    public float slowFactor = 0.5f; // 50% speed

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            JoystickPlayerExample player = other.GetComponent<JoystickPlayerExample>();
            if (player != null)
            {
                player.ModifySpeed(slowFactor);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            JoystickPlayerExample player = other.GetComponent<JoystickPlayerExample>();
            if (player != null)
            {
                player.ResetSpeed();
            }
        }
    }
}
