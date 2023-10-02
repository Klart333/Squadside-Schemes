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
    private float total = 0;

    private float timer = 0;

    public float Percent => !going ? 1.0f : (total - timer) / total;

    private void Update()
    {
        if (!going)
        {
            return;
        }

        timer += Time.deltaTime;

        float percent = (total - (timer + 1.0f)) / total;
        percent = Mathf.Clamp01(percent);

        SliderFill.fillAmount = percent;
        timerText.text = Mathf.RoundToInt(percent * total).ToString();
    }

    public void StartTimer(float roundLength)
    {
        going = true;
        total = roundLength;
        timer = 0;
    }

    public void StopTimer()
    {
        going = false;

        SliderFill.fillAmount = 0;
        timerText.text = "Battle Time!";
    }
}
