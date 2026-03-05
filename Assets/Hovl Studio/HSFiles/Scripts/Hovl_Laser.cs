using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System;
using UnityEngine;

public class Hovl_Laser : MonoBehaviour
{
    public int damageOverTime = 30;

    public GameObject HitEffect;
    public float HitOffset = 0;
    public bool useLaserRotation = false;

    public float MaxLength;
    private LineRenderer Laser;

    public float MainTextureLength = 1f;
    public float NoiseTextureLength = 1f;
    private Vector4 Length = new Vector4(1,1,1,1);
    //private Vector4 LaserSpeed = new Vector4(0, 0, 0, 0); {DISABLED AFTER UPDATE}
    //private Vector4 LaserStartSpeed; {DISABLED AFTER UPDATE}
    //One activation per shoot
    private bool LaserSaver = false;
    private bool UpdateSaver = false;
    
    // Smooth toggle variables
    public bool isLaserActive = true;
    public float fadeSpeed = 5f;
    private float currentAlpha = 1f;
    private float startWidth;
    private Color startColor;
    
    // Camera targeting
    public Camera targetCamera;
    public float cameraRayDistance = 100f;
    private Vector3 targetPoint;

    private ParticleSystem[] Effects;
    private ParticleSystem[] Hit;

    void Start ()
    {
        //Get LineRender and ParticleSystem components from current prefab;  
        Laser = GetComponent<LineRenderer>();
        Effects = GetComponentsInChildren<ParticleSystem>();
        Hit = HitEffect.GetComponentsInChildren<ParticleSystem>();
        //if (Laser.material.HasProperty("_SpeedMainTexUVNoiseZW")) LaserStartSpeed = Laser.material.GetVector("_SpeedMainTexUVNoiseZW");
        //Save [1] and [3] textures speed
        //{ DISABLED AFTER UPDATE}
        //LaserSpeed = LaserStartSpeed;
        
        // Store initial values for smooth toggling
        if (Laser != null)
        {
            startWidth = Laser.startWidth;
            startColor = Laser.material.color;
        }
        
        // Set initial alpha based on isLaserActive state
        currentAlpha = isLaserActive ? 1f : 0f;
    }

    void Update()
    {
        // Cast ray from camera to determine target point
        if (targetCamera != null)
        {
            Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit cameraHit;
            
            if (Physics.Raycast(ray, out cameraHit, cameraRayDistance))
            {
                targetPoint = cameraHit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(cameraRayDistance);
            }
            
            // Make laser look at target point
            Vector3 direction = targetPoint - transform.position;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        // Smooth alpha transition
        float targetAlpha = isLaserActive ? 1f : 0f;
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        
        // Update laser material and width based on alpha
        if (Laser != null)
        {
            Color color = startColor;
            color.a = currentAlpha;
            Laser.material.color = color;
            Laser.startWidth = startWidth * currentAlpha;
            Laser.endWidth = startWidth * currentAlpha;
        }
        
        // Stop particles when alpha is very low
        if (currentAlpha < 0.01f && !isLaserActive)
        {
            foreach (var ps in Effects)
            {
                if (ps.isPlaying) ps.Stop();
            }
            foreach (var ps in Hit)
            {
                if (ps.isPlaying) ps.Stop();
            }
        }
        
        //if (Laser.material.HasProperty("_SpeedMainTexUVNoiseZW")) Laser.material.SetVector("_SpeedMainTexUVNoiseZW", LaserSpeed);
        //SetVector("_TilingMainTexUVNoiseZW", Length); - old code, _TilingMainTexUVNoiseZW no more exist
        Laser.material.SetTextureScale("_MainTex", new Vector2(Length[0], Length[1]));                    
        Laser.material.SetTextureScale("_Noise", new Vector2(Length[2], Length[3]));
        //To set LineRender position
        if (Laser != null && UpdateSaver == false && currentAlpha > 0.01f)
        {
            Laser.SetPosition(0, transform.position);
            RaycastHit hit; //DELETE THIS IF YOU WANT USE LASERS IN 2D
            //ADD THIS IF YOU WANNT TO USE LASERS IN 2D: RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, MaxLength);       
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, MaxLength))//CHANGE THIS IF YOU WANT TO USE LASERRS IN 2D: if (hit.collider != null)
            {
                //End laser position if collides with object
                Laser.SetPosition(1, hit.point);

                    HitEffect.transform.position = hit.point + hit.normal * HitOffset;
                if (useLaserRotation)
                    HitEffect.transform.rotation = transform.rotation;
                else
                    HitEffect.transform.LookAt(hit.point + hit.normal);

                foreach (var AllPs in Effects)
                {
                    if (!AllPs.isPlaying) AllPs.Play();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, hit.point));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, hit.point));
                //Texture speed balancer {DISABLED AFTER UPDATE}
                //LaserSpeed[0] = (LaserStartSpeed[0] * 4) / (Vector3.Distance(transform.position, hit.point));
                //LaserSpeed[2] = (LaserStartSpeed[2] * 4) / (Vector3.Distance(transform.position, hit.point));
                //Destroy(hit.transform.gameObject); // destroy the object hit
                //hit.collider.SendMessage("SomeMethod"); // example
                if (hit.collider.CompareTag("possession"))
                {
                    PossessedItem possessedItem = hit.collider.GetComponent<PossessedItem>();
                    if (possessedItem != null)
                    {
                        if (possessedItem.isActiveAndEnabled && possessedItem.IsMatchingCore())
                        {
                            possessedItem.Cleanse();
                        }
                    }
                }

                if (hit.collider.CompareTag("Enemy"))
                {
                    // Apply damage over time to enemy
                    EnemyController enemyController = hit.collider.GetComponentInParent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.Die();
                    }
                }
            }
            else
            {
                //End laser position if doesn't collide with object
                var EndPos = transform.position + transform.forward * MaxLength;
                Laser.SetPosition(1, EndPos);
                HitEffect.transform.position = EndPos;
                foreach (var AllPs in Hit)
                {
                    if (AllPs.isPlaying) AllPs.Stop();
                }
                //Texture tiling
                Length[0] = MainTextureLength * (Vector3.Distance(transform.position, EndPos));
                Length[2] = NoiseTextureLength * (Vector3.Distance(transform.position, EndPos));
                //LaserSpeed[0] = (LaserStartSpeed[0] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
                //LaserSpeed[2] = (LaserStartSpeed[2] * 4) / (Vector3.Distance(transform.position, EndPos)); {DISABLED AFTER UPDATE}
            }
            //Insurance against the appearance of a laser in the center of coordinates!
            if (Laser.enabled == false && LaserSaver == false)
            {
                LaserSaver = true;
                Laser.enabled = true;
            }
        }  
    }

    public void DisablePrepare()
    {
        if (Laser != null)
        {
            Laser.enabled = false;
        }
        UpdateSaver = true;
        //Effects can = null in multiply shooting
        if (Effects != null)
        {
            foreach (var AllPs in Effects)
            {
                if (AllPs.isPlaying) AllPs.Stop();
            }
        }
    }
    
    public void ActivateLaser()
    {
        isLaserActive = true;
    }
    
    public void DeactivateLaser()
    {
        isLaserActive = false;
    }
    
    public void ToggleLaser()
    {
        isLaserActive = !isLaserActive;
    }
}
