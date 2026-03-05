using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class ChooseCandy : MonoBehaviour
{
    [Header("References")]
    public GameObject BucketCam;
    public GameObject CarCam;
    public Collider bucketCollider;
    public CinemachineBrain c_brain;

    public List<Collider> candyColliders;

    public float blendTime = 1.0f;

    private void Start()
    {
        c_brain.DefaultBlend.Time = blendTime;
    }

    public void startChoosing()
    {
        BucketCam.SetActive(true);
        CarCam.SetActive(false);
        bucketCollider.enabled = false;
        StartCoroutine(startChoosingCoroutine());
    }

    IEnumerator startChoosingCoroutine()
    {
        yield return new WaitForSeconds(1.1f);
        foreach(Collider collider in candyColliders)
        {
            collider.enabled = true;
        }
    }
}
