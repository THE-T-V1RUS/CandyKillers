using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public enum GameMode
    {
        FPS,
        Interact,
        Dialogue,
        Cutscene,
        None,
    }

    public static GameManager Instance { get; private set; }
    public TeletypeText dialogueText;
    public AudioManager audioManager;
    public TimeManager timeManager;
    public CurrentRoom currentRoom;
    public GameMode currentGameMode = GameMode.FPS;

    public CanvasGroup PlayerUI_CG, InteractUI_CG, Cellphone_CG;

    private void Awake()
    {
        // Check if instance already exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void ChangeGameMode(GameMode newGameMode)
    {
        currentGameMode = newGameMode;
        PlayerUI_CG.alpha = 0;
        InteractUI_CG.alpha = 0;
        Cellphone_CG.interactable = false;
        Cellphone_CG.blocksRaycasts = false;
        EventSystem.current.SetSelectedGameObject(null);

        switch (currentGameMode)
        {
            case GameMode.FPS:
                PlayerUI_CG.alpha = 1;
                break;
            case GameMode.Interact:
                Cellphone_CG.interactable = true;
                Cellphone_CG.blocksRaycasts = true;
                InteractUI_CG.alpha = 1;
                break;
        }
    }
}
