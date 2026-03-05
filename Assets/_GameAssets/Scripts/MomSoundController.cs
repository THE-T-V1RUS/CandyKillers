using UnityEngine;

public class MomSoundController : MonoBehaviour
{
    public CoreGameManager gameManager;
    public AudioClip snd_MomFall;

    public void PlayMomFallSoundEffect()
    {
        gameManager.PlaySoundEffect(snd_MomFall, 0.7f);
    }
}
