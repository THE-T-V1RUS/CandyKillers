using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float swayAmplitude = 2f;     // Degrees of sway on X
    public float swaySpeed = 1f;         // Oscillation speed
    public float yOffset = 0f;           // Additional Y rotation offset

    [Header("References")]
    public Transform player;                 // The player to follow for yaw
    public Transform playerCameraTransform;  // The Camera to use for tilt limiting
    public List<Transform> CandySpotTransforms;

    private float swayOffset;

    [Header("Look-Up Limiting")]
    public float lookUpStart = -10f;         // start tilting when looking up past this (degrees, negative)
    public float maxLookUpAngle = 60f;       // how far up we consider (positive magnitude)
    public float maxTiltWhenLookingUp = 30f; // how many degrees the bucket tilts at max look-up

    private void Start()
    {
        swayOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void LateUpdate()
    {
        if (player == null || playerCameraTransform == null)
            return;

        // --- PLAYER YAW (horizontal rotation only) ---
        float playerY = player.eulerAngles.y;

        // --- CAMERA PITCH (used for tilt limiting) ---
        float camX = playerCameraTransform.localEulerAngles.x;

        // Convert from [0,360) to [-180,180)
        if (camX > 180f)
            camX -= 360f;

        // --- SWAY ---
        float swayX = Mathf.Sin(Time.time * swaySpeed + swayOffset) * swayAmplitude;

        // --- TILT BASED ON CAMERA LOOK-UP ---
        float tiltAmount = 0f;
        if (camX < lookUpStart) // looking up
        {
            // camX goes from lookUpStart (e.g., -10) down to -maxLookUpAngle (e.g., -60)
            float t = Mathf.InverseLerp(lookUpStart, -maxLookUpAngle, camX);
            t = Mathf.Clamp01(t);
            tiltAmount = Mathf.Lerp(0f, maxTiltWhenLookingUp, t);
        }

        // --- FINAL ROTATION ---
        Quaternion baseYaw = Quaternion.Euler(0f, playerY + yOffset, 0f);
        float finalX = tiltAmount + swayX;

        // Compose yaw and pitch rotations
        transform.rotation = baseYaw * Quaternion.Euler(finalX, 0f, 0f);
    }
}
