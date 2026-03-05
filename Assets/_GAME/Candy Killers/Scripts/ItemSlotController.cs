using UnityEngine;

public class ItemSlotController : MonoBehaviour
{
    [SerializeField] Transform targetTransform;
    [SerializeField] float offset;

    private void LateUpdate()
    {
        this.transform.position = new Vector3(transform.position.x, targetTransform.position.y + offset, transform.position.z);
    }
}
