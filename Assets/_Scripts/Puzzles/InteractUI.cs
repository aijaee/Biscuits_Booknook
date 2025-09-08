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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var pm = PassageMenuUI.Instance;
            // if the passage menu GameObject is active, close it; otherwise interact
            if (pm != null && pm.gameObject.activeSelf)
            {
                pm.CloseMenu();
                ShowButton(false);
            }
            else
            {
                OnInteractButtonPressed();
            }
        }
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
