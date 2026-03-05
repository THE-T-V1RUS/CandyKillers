using System.Collections;
using TMPro;
using UnityEngine;

public class UpdateTimeText : MonoBehaviour
{
    bool isInitialized;
    GameManager gameManager;
    [SerializeField] TextMeshPro textMeshPro;
    [SerializeField] TextMeshProUGUI textMeshProUGUI;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    void Initialize()
    {
        gameManager = GameManager.Instance;
        if(gameManager == null)
        {
            Initialize();
            return;
        }

        StartCoroutine(UpdateTime());
    }

    IEnumerator UpdateTime()
    {
        while (true)
        {
            string TimeString = gameManager.timeManager.GetTimeString();
            if (textMeshProUGUI != null)
                textMeshProUGUI.text = TimeString;
            if (textMeshPro != null)
                textMeshPro.text = TimeString;
            yield return new WaitForSeconds(60);
        }
    }
}
