using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(HingeJoint))]
public class DoorHingeController : MonoBehaviour
{
    [SerializeField] private float pushTorque = 30f;
    [SerializeField] private float hingeLimitBuffer = 5f;

    private Rigidbody rb;
    private HingeJoint hinge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();

        // Smooth physics settings
        rb.mass = 10f;
        rb.angularDamping = 6f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Optional: tune hinge spring for soft return
        if (!hinge.useSpring)
        {
            JointSpring spring = new JointSpring
            {
                spring = 0f,      // 0 means no spring return
                damper = 0.5f,    // Optional smoothing
                targetPosition = 0f
            };
            hinge.spring = spring;
            hinge.useSpring = true;
        }

        // Make sure limits are being used
        hinge.useLimits = true;
    }

    public void PushDoor(Vector3 pushDirection)
    {
        if (hinge == null || rb == null || !hinge.useLimits)
            return;

        float angle = hinge.angle;
        JointLimits limits = hinge.limits;

        // Don’t apply torque if at or near hinge limits
        if (angle <= limits.min + hingeLimitBuffer || angle >= limits.max - hingeLimitBuffer)
            return;

        // Determine torque direction
        Vector3 localDir = transform.InverseTransformDirection(pushDirection.normalized);
        float torqueSign = Mathf.Sign(localDir.z); // Adjust axis if needed

        rb.AddTorque(hinge.axis * torqueSign * pushTorque, ForceMode.Force);
    }

    void FixedUpdate()
    {
        float angle = hinge.angle;
        JointLimits limits = hinge.limits;

        bool atLimit = angle <= limits.min + hingeLimitBuffer || angle >= limits.max - hingeLimitBuffer;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = !atLimit;
    }
}
