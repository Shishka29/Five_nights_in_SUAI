using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    private int time = 0;
    private Text clock;

    private void Awake()
    {
        clock = gameObject.GetComponent<Text>();
        InvokeRepeating("Timer", 60f, 60f);
    }

    public void Timer()
    {
        time++;
        clock.text = time.ToString() + " am";
        if (time >= 6)
        {
            CancelInvoke();
            Application.Quit();
        }
    }

}
