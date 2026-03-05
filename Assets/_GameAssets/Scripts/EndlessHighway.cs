using UnityEngine;
using System.Collections.Generic;

public class EndlessHighway : MonoBehaviour
{
    [Header("References")]
    public Transform player;              // The static car
    public List<Transform> roadSegments;  // Segments placed in sequence

    [Header("Movement Settings")]
    public float roadSpeed = 20f;         // Speed of movement
    public float recycleOffset = 30f;     // How far behind player before recycling

    public AudioSource ambience;
    [Range(0f, 1f)] [SerializeField] float targetAudioVolume;

    private float segmentLength;

    private void Start()
    {
        if (roadSegments == null || roadSegments.Count == 0)
        {
            Debug.LogError("No road segments assigned!");
            return;
        }

        // Automatically determine segment length
        segmentLength = CalculateSegmentLength(roadSegments[0]);
    }

    private void Update()
    {
        if(ambience.volume > targetAudioVolume)
        {
            var v = ambience.volume;
            v -= Time.deltaTime;
            ambience.volume = Mathf.Clamp(v, targetAudioVolume, 1);
        }

        if (roadSegments == null || roadSegments.Count == 0)
            return;

        // Move each segment toward the player
        foreach (Transform seg in roadSegments)
        {
            seg.position -= Vector3.forward * roadSpeed * Time.deltaTime;
        }

        // Recycle segments that go too far behind the player
        for (int i = 0; i < roadSegments.Count; i++)
        {
            Transform seg = roadSegments[i];

            // If behind player by more than segment length + offset → move to front
            if (seg.position.z < player.position.z - (segmentLength + recycleOffset))
            {
                float maxZ = GetFurthestSegmentZ();
                seg.position = new Vector3(seg.position.x, seg.position.y, maxZ + segmentLength);
            }
        }
    }

    private float GetFurthestSegmentZ()
    {
        float maxZ = float.MinValue;
        foreach (Transform seg in roadSegments)
        {
            if (seg.position.z > maxZ)
                maxZ = seg.position.z;
        }
        return maxZ;
    }

    private float CalculateSegmentLength(Transform segment)
    {
        // Try renderer bounds first
        Renderer rend = segment.GetComponentInChildren<Renderer>();
        if (rend != null)
            return rend.bounds.size.z;

        // Fallback: collider bounds
        Collider col = segment.GetComponentInChildren<Collider>();
        if (col != null)
            return col.bounds.size.z;

        // Fallback: distance between first two segments (if more than one)
        if (roadSegments.Count > 1)
            return Mathf.Abs(roadSegments[1].position.z - roadSegments[0].position.z);

        // Default safety value
        return 10f;
    }
}
