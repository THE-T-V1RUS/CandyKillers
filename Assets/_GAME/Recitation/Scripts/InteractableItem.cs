using UnityEngine;
using UnityEngine.Events;

// Example implementation of an interactable object
public class InteractableItem : MonoBehaviour, IInteractable
{
    [Header("Item Settings")]
    [SerializeField] private string itemName = "Item";
    [SerializeField] private bool canInteract = true;
    [SerializeField] private bool oneTimeUse = false;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool highlightWhenLookedAt = true;
    [SerializeField] private Color highlightColor = Color.yellow;
    private Renderer itemRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    
    [Header("Events")]
    [SerializeField] private UnityEvent onInteractEvent;
    
    private bool hasBeenUsed = false;
    
    private void Start()
    {
        // Cache renderer for highlighting
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer != null && itemRenderer.material != null)
        {
            originalColor = itemRenderer.material.color;
        }
    }
    
    public void OnInteract()
    {
        if (!canInteract || (oneTimeUse && hasBeenUsed))
            return;
        
        Debug.Log($"Interacting with {itemName}");
        
        // Invoke Unity Events
        onInteractEvent?.Invoke();
        
        // Mark as used if one-time use
        if (oneTimeUse)
        {
            hasBeenUsed = true;
        }
        
        // Add your custom interaction logic here
        PerformInteraction();
    }
    
    // Override this in derived classes for specific interaction behavior
    protected virtual void PerformInteraction()
    {
        // Default interaction behavior
        // Override this method in child classes for specific items
    }
    
    // Called by InteractionController when player looks at this object
    public void OnLookAt()
    {
        if (highlightWhenLookedAt && !isHighlighted && itemRenderer != null)
        {
            itemRenderer.material.color = highlightColor;
            isHighlighted = true;
        }
    }
    
    // Called when player looks away
    public void OnLookAway()
    {
        if (isHighlighted && itemRenderer != null)
        {
            itemRenderer.material.color = originalColor;
            isHighlighted = false;
        }
    }
    
    // Enable/disable interaction
    public void SetInteractable(bool state)
    {
        canInteract = state;
    }
    
    // Check if item can be interacted with
    public bool CanInteract()
    {
        return canInteract && !(oneTimeUse && hasBeenUsed);
    }
}
