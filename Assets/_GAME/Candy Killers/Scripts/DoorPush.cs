using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class DoorPusher : MonoBehaviour
{
    [SerializeField] private float pushStrength = 1f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Door"))
            return;

        DoorHingeController door = hit.collider.GetComponent<DoorHingeController>();
        if (door == null)
            return;

        // Calculate push direction (player -> door)
        Vector3 pushDir = hit.collider.transform.position - transform.position;
        pushDir.y = 0;

        door.PushDoor(pushDir * pushStrength);
    }
}
