using UnityEngine;

public class DiscordButton : MonoBehaviour
{
    private const string discordLink = "https://discord.gg/8jX8aBmQks";

    public void OpenLink()
    {
        Application.OpenURL(discordLink);
    }
}
