using UnityEngine;

public class Headphones : MonoBehaviour
{
    public void OnEquip()
    {
        Recitation_AudioManager.Instance.SetAudioMode(Recitation_AudioManager.AudioMode.Headphones);
    }

    public void OnUnequip()
    {
        Recitation_AudioManager.Instance.SetAudioMode(Recitation_AudioManager.AudioMode.Normal);
    }
}
