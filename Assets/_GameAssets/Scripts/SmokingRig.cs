using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SmokingRig : MonoBehaviour
{
    [Header("Rig Settings")]
    public Rig smokeRig;
    public Rig headRig;
    [Range(0f, 1f)] public float targetWeight = 0f;
    public float smoothTime = 0.2f; // smaller = faster smoothing

    [Header("Target Settings")]
    public Transform targetTransform;
    public float targetLocalZOffset = 0.1f;

    [Header("Visual Settings")]
    public Renderer smokeRenderer;
    public Color idleEmission = Color.black;
    public Color dragEmission = Color.white;
    public float colorChangeSpeed = 2f;

    private Vector3 defaultLocalPosition;
    private float currentWeight;
    private float weightVelocity = 0f; // for SmoothDamp

    public float MinDelay;
    public float MaxDelay;

    private void Start()
    {
        if (targetTransform != null)
            defaultLocalPosition = targetTransform.localPosition;

        currentWeight = smokeRig.weight;

        if (smokeRenderer != null)
        {
            Material mat = smokeRenderer.material;
            mat.SetColor("_EmissionColor", idleEmission);
            mat.EnableKeyword("_EMISSION");
        }

        StartCoroutine(SmokeCoroutine());
    }

    private void Update()
    {
        if (smokeRig == null || headRig == null)
            return;

        // Smoothly interpolate weight using SmoothDamp
        currentWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref weightVelocity, smoothTime);
        smokeRig.weight = currentWeight;

        // Head rig is inverse of smoke rig
        headRig.weight = 1f - currentWeight;

        if (targetTransform != null)
        {
            float offsetBlend = 1f - currentWeight;
            Vector3 targetPos = defaultLocalPosition + new Vector3(0f, 0f, targetLocalZOffset * offsetBlend);
            targetTransform.localPosition = Vector3.Lerp(targetTransform.localPosition, targetPos, Time.deltaTime * 5f);
        }
    }

    private IEnumerator SmokeCoroutine()
    {
        while (true)
        {
            float delayTimer = Random.Range(MinDelay, MaxDelay);
            yield return new WaitForSeconds(delayTimer);

            targetWeight = 1f;
            yield return new WaitForSeconds(1.3f);

            yield return StartCoroutine(ChangeEmission(smokeRenderer, dragEmission, colorChangeSpeed));
            yield return new WaitForSeconds(1.3f);

            yield return StartCoroutine(ChangeEmission(smokeRenderer, idleEmission, colorChangeSpeed));
            targetWeight = 0f;
        }
    }

    private IEnumerator ChangeEmission(Renderer rend, Color targetColor, float speed)
    {
        if (rend == null) yield break;

        Material mat = rend.material;
        Color startColor = mat.GetColor("_EmissionColor");
        float t = 0f;
        mat.EnableKeyword("_EMISSION");

        while (t < 1f)
        {
            t += Time.deltaTime * speed;
            mat.SetColor("_EmissionColor", Color.Lerp(startColor, targetColor, t));
            yield return null;
        }
        mat.SetColor("_EmissionColor", targetColor);
    }
}
