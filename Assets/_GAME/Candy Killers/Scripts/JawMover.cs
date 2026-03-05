using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JawMover : MonoBehaviour
{
    public bool isDebugMode = false;
    public Transform jawBone;

    [Header("Editor Settings")]
    public Vector3 editorOpenRotation = new Vector3(0, -20, 0);
    [Range(0.01f, 100f)] public float editorSensitivity = 0.12f;
    public float editorMovementSpeed = 78.5f;

    [Header("Build Settings")]
    public Vector3 buildOpenRotation = new Vector3(0, -20, 0);
    [Range(0.1f, 100f)] public float buildSensitivity = 6f;
    public float buildMovementSpeed = 78.5f;

    [Header("Build Hotkeys")]
    public KeyCode sensitivityUpKey = KeyCode.KeypadPlus;
    public KeyCode sensitivityDownKey = KeyCode.KeypadMinus;
    public KeyCode speedUpKey = KeyCode.Keypad8;
    public KeyCode speedDownKey = KeyCode.Keypad2;
    public float adjustmentStep = 0.5f;

    private AudioSource audioSource;
    private Quaternion closedRotation;
    private float[] samples = new float[1024];
    private float volume;
    private float volumeVelocity;
    private Vector3 currentOpenRotation;
    private float currentSensitivity;
    private float currentMovementSpeed;
    private float adjustmentTimer;
    private string lastAdjustment;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        closedRotation = jawBone.localRotation;

#if UNITY_EDITOR
        ApplyEditorSettings();
#else
        ApplyBuildSettings();
#endif
    }

    void Update()
    {
        HandleAudioInput();
        HandleBuildAdjustments();
    }

    void HandleAudioInput()
    {
        if (!audioSource.isPlaying)
        {
            jawBone.localRotation = closedRotation;
            return;
        }

        audioSource.GetOutputData(samples, 0);
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += Mathf.Abs(samples[i]);
        }
        float targetVolume = sum / samples.Length;
        volume = Mathf.SmoothDamp(volume, targetVolume, ref volumeVelocity, 0.05f);
    }

    void HandleBuildAdjustments()
    {
#if !UNITY_EDITOR
        bool adjusted = false;
        
        // Sensitivity adjustments
        if (Input.GetKey(sensitivityUpKey))
        {
            buildSensitivity += adjustmentStep * Time.deltaTime * 10f;
            adjusted = true;
            lastAdjustment = $"Sensitivity+: {buildSensitivity:F2}";
        }
        else if (Input.GetKey(sensitivityDownKey))
        {
            buildSensitivity = Mathf.Max(0.1f, buildSensitivity - adjustmentStep * Time.deltaTime * 10f);
            adjusted = true;
            lastAdjustment = $"Sensitivity-: {buildSensitivity:F2}";
        }

        // Speed adjustments
        if (Input.GetKey(speedUpKey))
        {
            buildMovementSpeed += adjustmentStep * Time.deltaTime * 50f;
            adjusted = true;
            lastAdjustment = $"Speed+: {buildMovementSpeed:F1}";
        }
        else if (Input.GetKey(speedDownKey))
        {
            buildMovementSpeed = Mathf.Max(1f, buildMovementSpeed - adjustmentStep * Time.deltaTime * 50f);
            adjusted = true;
            lastAdjustment = $"Speed-: {buildMovementSpeed:F1}";
        }

        if (adjusted)
        {
            adjustmentTimer = 1f;
            ApplyBuildSettings(); // Immediately apply changes
        }
        else if (adjustmentTimer > 0)
        {
            adjustmentTimer -= Time.deltaTime;
        }
#endif
    }

    void LateUpdate()
    {
        float t = Mathf.Clamp01(volume * currentSensitivity);
        jawBone.localRotation = Quaternion.Slerp(
            jawBone.localRotation,
            closedRotation * Quaternion.Euler(currentOpenRotation * t),
            Time.deltaTime * currentMovementSpeed
        );
    }

    void ApplyEditorSettings()
    {
        currentOpenRotation = editorOpenRotation;
        currentSensitivity = editorSensitivity;
        currentMovementSpeed = editorMovementSpeed;
        Debug.Log("Applied EDITOR settings");
    }

    void ApplyBuildSettings()
    {
        currentOpenRotation = buildOpenRotation;
        currentSensitivity = buildSensitivity;
        currentMovementSpeed = buildMovementSpeed;
    }

    void OnGUI()
    {
        if (!isDebugMode) return;

        GUI.skin.label.fontSize = 20;
        GUI.color = Color.cyan;

        // Current settings
        GUI.Label(new Rect(10, 10, 600, 30), $"Platform: {(Application.isEditor ? "EDITOR" : "BUILD")}");
        GUI.Label(new Rect(10, 40, 600, 30), $"Sensitivity: {currentSensitivity:F2}");
        GUI.Label(new Rect(10, 70, 600, 30), $"Speed: {currentMovementSpeed:F1}");
        GUI.Label(new Rect(10, 100, 600, 30), $"Jaw Angle: {jawBone.localEulerAngles.y:F1}°");

        // Build controls help
#if !UNITY_EDITOR
        GUI.Label(new Rect(10, 130, 600, 30), "Numpad +/-: Adjust Sensitivity");
        GUI.Label(new Rect(10, 160, 600, 30), "Numpad 8/2: Adjust Speed");
        
        // Adjustment feedback
        if (adjustmentTimer > 0)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(Screen.width/2 - 100, Screen.height - 60, 200, 30), lastAdjustment);
        }
#endif
    }
}