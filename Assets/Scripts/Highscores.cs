/*using UnityEngine;
using System;
using System.Collections;

public class Highscores
{
    const string privateCode = "uQ41Aoh2gkCawmGOQ2nacQcLQw_KbVsUS4YAjdtxpUvw";
    const string publicCode = "652f8e6d8f40bb11fc4b53ef";
    const string webURL = "http://dreamlo.com/lb/";

    public PlayerRank[] rankList;
    private MonoBehaviour behaviour;

    public Highscores(MonoBehaviour behaviour)
    {
        this.behaviour = behaviour;
    }

    public void AddNewHighscore(string username, int score)
    {
        behaviour.StartCoroutine(UploadNewHighscore(username, score));
    }

    private IEnumerator UploadNewHighscore(string username, int score)
    {
        if (username == "dumbass didn't enter a name")
        {
            username = UnityEngine.Random.Range(1001, 1000000).ToString();
        }

        WWW www = new WWW(webURL + privateCode + "/add/" + WWW.EscapeURL(username) + "/" + score);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
            Debug.Log("Upload Successful");
        else
        {
            Debug.Log("Error uploading: " + www.error);
        }
    }

    public void DownloadHighscores(Action<PlayerRank[]> action)
    {
        behaviour.StartCoroutine(DownloadHighscoresFromDatabase(action));
    }

    private IEnumerator DownloadHighscoresFromDatabase(Action<PlayerRank[]> action)
    {
        WWW www = new WWW(webURL + publicCode + "/pipe/");
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            FormatHighscores(www.text);
            action(rankList);
        }
        else
        {
            Debug.Log("Error Downloading: " + www.error);
        }
    }

    private void FormatHighscores(string textStream)
    {
        string[] entries = textStream.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        rankList = new PlayerRank[entries.Length];

        for (int i = 0; i < entries.Length; i++)
        {
            string[] entryInfo = entries[i].Split(new char[] { '|' });
            string username = entryInfo[0];
            username = username.Replace('+', ' ');
            if (int.TryParse(username, out int result))
            {
                if (result > 1000)
                {
                    username = "dumbass didn't enter a name";
                }
            }
            int score = int.Parse(entryInfo[1]);
            rankList[i] = new PlayerRank(username, score);
        }
    }
}

*/