using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Reflector : MonoBehaviour
{
    public float activeDuration = 0.4f; 

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;     
    }

    void Update()
    {

        if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame))
        {
            col.enabled = true;   
            Invoke(nameof(Deactivate), activeDuration);
        }
    }

    void Deactivate()
    {
        col.enabled = false;    
    }
}
