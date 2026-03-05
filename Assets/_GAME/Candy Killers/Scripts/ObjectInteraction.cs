using UnityEngine;
using UnityEngine.Events;

public class ObjectInteraction : MonoBehaviour
{
    [SerializeField] UnityEvent interactionEvent;

    public void PerformInteractionEvent()
    {
        interactionEvent.Invoke();
    }
}
