using TMPro;
using UnityEngine;

public class UIDisplayPlayerCount : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI inQueueText;

    [SerializeField]
    private TextMeshProUGUI inGameText;

    [SerializeField]
    private TextMeshProUGUI countdownText;

    [SerializeField]
    private float updateInterval = 10f;

    private float timer = 0;

    private void Update()
    {
        timer -= Time.deltaTime;
        countdownText.text = timer.ToString("f0");

        if (timer <= 0.0f)
        {
            timer = updateInterval;

            PlayerCountManager.Instance.Highscores.DownloadHighscores(DisplayCount);
        }
    }

    public void DisplayCount(PlayerCountHighscore[] scores)
    {
        if (scores.Length > 0)
        {
            inQueueText.text = scores[0].Amount.ToString();
        }

        if (scores.Length > 1)
        {
            inGameText.text = scores[1].Amount.ToString();
        }
    }
}
