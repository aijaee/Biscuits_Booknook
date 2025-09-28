using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    [Range(0f, 2f)]
    public float speedFactor = 1.5f; // 150% speed

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            JoystickPlayerExample player = other.GetComponent<JoystickPlayerExample>();
            if (player != null)
            {
                player.ModifySpeed(speedFactor);
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
