using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] int current_hour, current_minute;
    private Coroutine time_coroutine;

    private void Start()
    {
        RestartTimeAdvancement();   
    }

    public void SetTime(int hour, int minute)
    {
        current_hour = hour;
        current_minute = minute;
    }

    public void RestartTimeAdvancement()
    {
        if(time_coroutine != null)
            StopCoroutine(time_coroutine);
        time_coroutine = StartCoroutine(AdvanceTime());
    }

    public string GetTimeString()
    {
        return current_hour + ":" + current_minute.ToString("00");
    }

    IEnumerator AdvanceTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            current_minute += 1;
            if (current_minute > 59)
            {
                current_minute = 0;
                current_hour += 1;
                if (current_hour > 12)
                    current_hour = 1;
            }
        }
    }
}
