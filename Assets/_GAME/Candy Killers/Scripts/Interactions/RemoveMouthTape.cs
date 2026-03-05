using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class RemoveMouthTape : MonoBehaviour
{
    [SerializeField] CharacterFaceController characterFaceController;
    [SerializeField] AudioSource eric_AudioSource;
    [SerializeField] GameObject obj_ductTape;
    [SerializeField] AudioClip snd_RemoveTape, voiceLine_Eric_0000;

    private CharacterFaceController.Expression previousExpression;

    public void RemoveTape()
    {
        previousExpression = characterFaceController.currentExpression;
        GameManager gameManager = GameManager.Instance;
        gameManager.audioManager.PlaySFX(snd_RemoveTape);
        obj_ductTape.SetActive(false);
        characterFaceController.ChangeExpression(CharacterFaceController.Expression.Pain);
        eric_AudioSource.clip = voiceLine_Eric_0000;
        eric_AudioSource.Play();
        StartCoroutine(RemovePainExpression());
    }

    IEnumerator RemovePainExpression()
    {
        yield return new WaitForSeconds(1);
        characterFaceController.ChangeExpression(previousExpression);
    }
}
