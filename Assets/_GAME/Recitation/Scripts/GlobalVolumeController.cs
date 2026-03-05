using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    
    private FilmGrain filmGrain;
    private ChromaticAberration chromaticAberration;

    [Header("Default Intensities")]
    public float defaultFilmGrainIntensity = 0.3f;
    public float defaultChromaticAberrationIntensity = 0f;
    
    [Header("Enemy Detection")]
    [SerializeField] private float detectionRadius = 20f;
    [SerializeField] private float maxEffectDistance = 5f; // Distance at which effects are at max intensity
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Effect Intensities")]
    [SerializeField] private float maxFilmGrainIntensity = 1f;
    [SerializeField] private float maxChromaticAberrationIntensity = 1f;
    [SerializeField] private float effectTransitionSpeed = 2f; // Speed at which effects transition
    
    private EnemyController detectedEnemy;
    private float currentFilmGrainIntensity;
    private float currentChromaticAberrationIntensity;
    
    private void Awake()
    {
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out filmGrain);
            globalVolume.profile.TryGet(out chromaticAberration);
        }
        
        // Initialize current intensities to default values
        currentFilmGrainIntensity = defaultFilmGrainIntensity;
        currentChromaticAberrationIntensity = defaultChromaticAberrationIntensity;
    }
    
    private void Update()
    {
        DetectNearbyEnemies();
        UpdateEffectsBasedOnEnemyDistance();
    }
    
    private void DetectNearbyEnemies()
    {
        // Sphere cast around player to find enemies
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        
        if (hitColliders.Length > 0)
        {
            // Find the closest enemy
            float closestDistance = float.MaxValue;
            EnemyController closestEnemy = null;
            
            foreach (Collider hitCollider in hitColliders)
            {
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null && !enemy.IsInDeathState())
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
            
            detectedEnemy = closestEnemy;
        }
        else
        {
            detectedEnemy = null;
        }
    }
    
    private void UpdateEffectsBasedOnEnemyDistance()
    {
        float targetFilmGrain;
        float targetChromaticAberration;
        
        if (detectedEnemy != null && !detectedEnemy.IsInDeathState())
        {
            // Calculate distance to enemy
            float distance = Vector3.Distance(transform.position, detectedEnemy.transform.position);
            
            // Calculate intensity based on distance (closer = higher intensity)
            // t = 0 when distance >= detectionRadius, t = 1 when distance <= maxEffectDistance
            float t = Mathf.Clamp01(1f - (distance - maxEffectDistance) / (detectionRadius - maxEffectDistance));
            
            // Lerp from default to max intensity
            targetFilmGrain = Mathf.Lerp(defaultFilmGrainIntensity, maxFilmGrainIntensity, t);
            targetChromaticAberration = Mathf.Lerp(defaultChromaticAberrationIntensity, maxChromaticAberrationIntensity, t);
        }
        else
        {
            // No enemy detected or enemy is dead, return to default values
            targetFilmGrain = defaultFilmGrainIntensity;
            targetChromaticAberration = defaultChromaticAberrationIntensity;
        }
        
        // Smoothly lerp current values toward target values
        currentFilmGrainIntensity = Mathf.Lerp(currentFilmGrainIntensity, targetFilmGrain, effectTransitionSpeed * Time.deltaTime);
        currentChromaticAberrationIntensity = Mathf.Lerp(currentChromaticAberrationIntensity, targetChromaticAberration, effectTransitionSpeed * Time.deltaTime);
        
        // Apply the smoothly transitioning values
        SetFilmGrainIntensity(currentFilmGrainIntensity);
        SetChromaticAberrationIntensity(currentChromaticAberrationIntensity);
    }
    
    public void SetFilmGrainIntensity(float intensity)
    {
        if (filmGrain != null)
        {
            filmGrain.intensity.value = Mathf.Clamp01(intensity);
        }
    }
    
    public void SetChromaticAberrationIntensity(float intensity)
    {
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Clamp01(intensity);
        }
    }
    
    public float GetFilmGrainIntensity()
    {
        return filmGrain != null ? filmGrain.intensity.value : 0f;
    }
    
    public float GetChromaticAberrationIntensity()
    {
        return chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
    }
}
