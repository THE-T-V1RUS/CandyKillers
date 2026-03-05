using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Hertzole.GoldPlayer;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TeletypeText : MonoBehaviour
{
    [TextArea]
    [SerializeField] private List<string> messages;
    [SerializeField] private float characterDelay = 0.05f;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip letterSound;
    [SerializeField] private bool playSound = true;
    [SerializeField] private CanvasGroup dialogue_CanvasGroup, playerUI_CanvasGroup;
    [SerializeField] private GoldPlayerController playerController;

    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;
    private GameManager.GameMode previousGameMode;
    GameManager gameManager;

    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        gameManager = GameManager.Instance;
        if(gameManager == null)
        {
            Initialize();
            return;
        }
    }

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void StartTeletype(List<string> messageList, GameManager.GameMode customGameMode = GameManager.GameMode.None)
    {
        previousGameMode = gameManager.currentGameMode;
        if (customGameMode != GameManager.GameMode.None)
            previousGameMode = customGameMode;
        gameManager.ChangeGameMode(GameManager.GameMode.Dialogue);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeMultipleMessages(messageList));
    }

    private IEnumerator TypeMultipleMessages(List<string> messageList)
    {
        playerController.Movement.CanMoveAround = false;
        playerController.Camera.CanLookAround = false;

        // Fade in
        while (dialogue_CanvasGroup.alpha < 1)
        {
            dialogue_CanvasGroup.alpha += Time.deltaTime * 4f;
            yield return null;
        }

        foreach (string message in messageList)
        {
            yield return StartCoroutine(TypeText(message));
            yield return StartCoroutine(WaitForPlayerInput());
        }

        if(previousGameMode == GameManager.GameMode.FPS)
        {
            playerController.Movement.CanMoveAround = true;
            playerController.Camera.CanLookAround = true;
        }

        gameManager.ChangeGameMode(previousGameMode);

        // Fade out
        while (dialogue_CanvasGroup.alpha > 0)
        {
            dialogue_CanvasGroup.alpha -= Time.deltaTime * 4f;
            yield return null;
        }

        textComponent.text = "";
    }

    private IEnumerator TypeText(string message)
    {
        textComponent.text = message;
        textComponent.maxVisibleCharacters = 0;

        int totalChars = message.Length;

        for (int i = 0; i < totalChars; i++)
        {
            textComponent.maxVisibleCharacters = i + 1;

            if (playSound && letterSound != null && audioSource != null && !char.IsWhiteSpace(message[i]))
            {
                audioSource.PlayOneShot(letterSound);
            }

            yield return new WaitForSeconds(characterDelay);
        }
    }

    private IEnumerator WaitForPlayerInput()
    {
        // Wait until any key or mouse button is pressed
        while (!Input.anyKeyDown)
            yield return null;

        // Optional: wait until key is released before continuing
        yield return new WaitForEndOfFrame();
    }
}
