using System.Collections;
using UnityEngine;

public class GetBurrito : MonoBehaviour
{
    [SerializeField] GameObject burritoPrefab;
    [SerializeField] AudioSource burritoAudioSource;
    [SerializeField] AudioClip snd_getBurrito;
    [SerializeField] BoxCollider interactionCollider, MicrowaveInteractionCollider;

    public void SpawnBurittoPrefabOnPlayer()
    {
        StartCoroutine(SpawnBurritoCoruotine());
    }

    IEnumerator SpawnBurritoCoruotine()
    {
        interactionCollider.enabled = false;
        burritoAudioSource.clip = snd_getBurrito;
        burritoAudioSource.Play();
        yield return new WaitForSeconds(0.5f);
        burritoPrefab.SetActive(true);
        MicrowaveInteractionCollider.enabled = true;
    }
}
