using UnityEngine;

public class InteractUI : MonoBehaviour
{
    public static InteractUI Instance { get; private set; }

    public GameObject interactButtonObject;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        interactButtonObject.SetActive(false);
    }

    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void ShowButton(bool show)
    {
        if (interactButtonObject != null)
            interactButtonObject.SetActive(show);
    }

    // Hook this up to your On-Screen Button's OnClick() event
    public void OnInteractButtonPressed()
    {
        if (currentInteractable != null)
        {
            currentInteractable.TryInteract();
        }
    }
}
