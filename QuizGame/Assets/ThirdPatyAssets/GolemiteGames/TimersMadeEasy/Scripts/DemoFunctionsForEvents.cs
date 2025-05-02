using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DemoFunctionsForEvents : MonoBehaviour
{
    public GameObject startedText;
    public GameObject finishedText;

    public Timer timer;
    public Timer dialTimer;

    private void Start()
    {
        timer.StartTimerCustom(5);
    }
    public void TimerStart()
    {
        startedText.SetActive(true);
        finishedText.SetActive(false);
    }
    public void TimerEnd()
    {
        finishedText.SetActive(true);
        startedText.SetActive(false);
    }

    public void addTime()
    {
        timer.AddTime(2);
        dialTimer.AddTime(2);
    }
    public void removeTime()
    {
        timer.RemoveTime(2);
        dialTimer.RemoveTime(2);
    }
}
