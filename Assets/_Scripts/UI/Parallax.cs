using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Vector2 parallaxMultiplier = new Vector2(0.5f, 0.5f);

    private Vector3 lastCameraPosition;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        // Match initial camera position
        lastCameraPosition = cameraTransform.position;

        // Optional: set initial background position based on camera
        transform.position = new Vector3(
            cameraTransform.position.x * parallaxMultiplier.x,
            cameraTransform.position.y * parallaxMultiplier.y,
            transform.position.z // preserve background z
        );
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(
            deltaMovement.x * parallaxMultiplier.x,
            deltaMovement.y * parallaxMultiplier.y,
            0f
        );

        lastCameraPosition = cameraTransform.position;
    }
}
