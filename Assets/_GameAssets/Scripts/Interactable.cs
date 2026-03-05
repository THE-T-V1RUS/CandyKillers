using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Settings")]
    public string interactText = string.Empty;

    [Header("Events")]
    public UnityEvent onInteract;

    // Call this when the player interacts
    public void Interact()
    {
        if (onInteract != null)
            onInteract.Invoke();
    }
}