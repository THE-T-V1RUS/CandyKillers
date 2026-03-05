using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FollowMouseUI : MonoBehaviour
{
    private RectTransform rectTransform;
    public CanvasGroup InteractionUI_CanvasGroup;

    [SerializeField] private float fadeDuration = 0f;

    private bool isVisible = false;
    private Coroutine fadeCoroutine;
    GameManager gameManager;

    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Initialize();
            return;
        }
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (gameManager == null) return;

        rectTransform.position = Input.mousePosition;

        bool shouldBeVisible = gameManager.currentGameMode == GameManager.GameMode.Interact;

        if (shouldBeVisible != isVisible)
        {
            isVisible = shouldBeVisible;
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeCanvasGroup(InteractionUI_CanvasGroup, isVisible ? 1f : 0f, fadeDuration));
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0;
        canvasGroup.interactable = targetAlpha > 0;
    }
}