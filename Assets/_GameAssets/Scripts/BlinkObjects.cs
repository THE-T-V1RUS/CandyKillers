using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkObjects : MonoBehaviour
{
    [Header("Object Groups")]
    public List<GameObject> firstGroup;
    public List<GameObject> secondGroup;

    [Header("Blink Settings")]
    public int blinkCount = 2;      // Number of on/off blinks per group
    public float blinkDelay = 0.3f; // Time between blinks
    public float cycleDelay = 0.5f; // Delay between groups

    void Start()
    {
        // Make sure everything starts off
        SetGroupActive(firstGroup, false);
        SetGroupActive(secondGroup, false);

        // Begin the repeating blink cycle
        StartCoroutine(BlinkLoop());
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            // Blink first group
            yield return StartCoroutine(BlinkGroup(firstGroup));

            // Optional pause before next group
            yield return new WaitForSeconds(cycleDelay);

            // Blink second group
            yield return StartCoroutine(BlinkGroup(secondGroup));

            // Optional pause before restarting the cycle
            yield return new WaitForSeconds(cycleDelay);
        }
    }

    IEnumerator BlinkGroup(List<GameObject> group)
    {
        for (int i = 0; i < blinkCount; i++)
        {
            SetGroupActive(group, true);
            yield return new WaitForSeconds(blinkDelay);

            SetGroupActive(group, false);
            yield return new WaitForSeconds(blinkDelay);
        }
    }

    void SetGroupActive(List<GameObject> group, bool state)
    {
        foreach (GameObject obj in group)
        {
            if (obj != null)
                obj.SetActive(state);
        }
    }
}
