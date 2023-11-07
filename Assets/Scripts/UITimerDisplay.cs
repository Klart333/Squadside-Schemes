using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UITimerDisplay : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timerText;

    [SerializeField]
    private Image SliderFill;

    private bool going = false;
    private float roundLength = 0;

    private float timer = 0;
    private bool isCreepRound = true;

    public float Percent => !going ? 1.0f : (roundLength - timer) / roundLength;

    private void Update()
    {
        if (!going)
        {
            return;
        }

        timer += Time.deltaTime;

        float percent = (roundLength - (timer + 1.5f)) / roundLength;
        percent = Mathf.Clamp01(percent);

        SliderFill.fillAmount = percent;
        if (percent * roundLength <= 0)
        {
            timerText.text = "Loading boards...";
        }
        else
        {
            timerText.text = Mathf.RoundToInt(percent * roundLength).ToString();
        }
    }

    public void StartTimer(float roundLength)
    {
        going = true;
        this.roundLength = roundLength;
        timer = 0;
    }

    public void StopTimer()
    {
        going = false;

        SliderFill.fillAmount = 0;

        if (isCreepRound)
        {
            timerText.text = "Mob Time!";
        }
        else
        {
            timerText.text = "Battle Time!";
        }

        isCreepRound = !isCreepRound;
    }

    internal void ShowOvertime()
    {
        timerText.text = "Overtime!";
    }
}
