using UnityEngine;

public class Recitation_GameManager : MonoBehaviour
{
    public static Recitation_GameManager Instance { get; private set; }
    public CameraEffectsHandler cameraEffectsHandler;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
