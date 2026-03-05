using UnityEngine;
using System.Collections;

public class MemoryController : MonoBehaviour
{
    public CoreGameManager gameManager;

    [Header("UI Elements")]
    public CanvasGroup memoryCanvas;
    public CanvasGroup memoryTutorial;

    [Header("Settings")]
    public float fadeSpeed = 1.5f;

    [Header("Memory Objects")]
    public Transform memoryParent;

    private float tutorialTargetAlpha;
    private float memoryTargetAlpha;

    private void Start()
    {
        RandomizeMemoryChildren();
        tutorialTargetAlpha = memoryTutorial.alpha;
        memoryTargetAlpha = memoryCanvas.alpha;
        StartCoroutine(FadeRoutine());
    }

    private void RandomizeMemoryChildren()
    {
        int childCount = memoryParent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = memoryParent.GetChild(i);
            child.SetSiblingIndex(Random.Range(0, childCount));
        }
    }

    private void Update()
    {
        // Update target alphas based on input/state
        tutorialTargetAlpha = gameManager.canAccessMemory ? 1f : 0f;
        memoryTargetAlpha = (gameManager.canAccessMemory && Input.GetKey(KeyCode.M)) ? 1f : 0f;
    }

    private IEnumerator FadeRoutine()
    {
        while (true)
        {
            // Smoothly fade both canvases toward their targets
            memoryTutorial.alpha = Mathf.MoveTowards(memoryTutorial.alpha, tutorialTargetAlpha, fadeSpeed * Time.deltaTime);
            memoryCanvas.alpha = Mathf.MoveTowards(memoryCanvas.alpha, memoryTargetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
