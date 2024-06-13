using UnityEngine;
using System;
using System.Collections;
using Sirenix.Utilities;
using UnityEngine.Networking;

public class Highscores
{
    public event Action OnPlayerCountUpdated;

    const string privateCode = "uQ41Aoh2gkCawmGOQ2nacQcLQw_KbVsUS4YAjdtxpUvw";
    const string publicCode = "652f8e6d8f40bb11fc4b53ef";
    const string webURL = "http://dreamlo.com/lb/";

    public PlayerCountHighscore[] rankList;
    private MonoBehaviour behaviour;

    public Highscores(MonoBehaviour behaviour)
    {
        this.behaviour = behaviour;
    }

    public void AddNewHighscore(string username, int score)
    {
        behaviour.StartCoroutine(UploadNewHighscore(username, score));
    }

    public IEnumerator UpdatePlayerCount(int index, int change)
    {
        yield return DownloadHighscoresFromDatabase(null);
        int oldScore = rankList[index].Amount;

        string url = webURL + privateCode + "/delete/" + UnityWebRequest.EscapeURL(index.ToString());
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
        }

        int amount = Mathf.Max(oldScore + change, 0);
        yield return UploadNewHighscore(index.ToString(), amount);

        OnPlayerCountUpdated?.Invoke();
    }

    private IEnumerator UploadNewHighscore(string username, int score)
    {
        string url = webURL + privateCode + "/add/" + UnityWebRequest.EscapeURL(username) + "/" + score;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload Successful: Username: " + username + ", Score: " + score);
            }
            else
            {
                Debug.Log("Error uploading: " + webRequest.error);
            }
        }
    }

    public void DownloadHighscores(Action<PlayerCountHighscore[]> action)
    {
        behaviour.StartCoroutine(DownloadHighscoresFromDatabase(action));
    }

    private IEnumerator DownloadHighscoresFromDatabase(Action<PlayerCountHighscore[]> action)
    {
        string url = webURL + publicCode + "/pipe/";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                FormatHighscores(webRequest.downloadHandler.text);

                if (action != null)
                {
                    action(rankList);
                }
            }
            else
            {
                Debug.Log("Error Downloading: " + webRequest.error);
                // Handle error downloading here
            }
        }
    }

    private void FormatHighscores(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        rankList = new PlayerCountHighscore[entries.Length];

        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            username = username.Replace('+', ' ');
            int score = int.Parse(entryInfo[1]);
            rankList[i] = new PlayerCountHighscore(username, score);
        }

        rankList.Sort((x, y) => x.Index.CompareTo(y.Index));
    }
}

public struct PlayerCountHighscore
{
    public int Index;
    public int Amount;

    public PlayerCountHighscore(string name, int amount)
    {
        if (!int.TryParse(name[0].ToString(), out Index))
        {
            Debug.LogError("Parsing failed :(");
            Index = -1;
        }

        Amount = amount;
    }
}