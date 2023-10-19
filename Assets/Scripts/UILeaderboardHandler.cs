using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILeaderboardHandler : MonoBehaviour
{
    [SerializeField]
    private UIPlayerScore playerScorePrefab;

    [SerializeField]
    private GameObject loadingPanel;

    [SerializeField]
    private float cellHeight = 60;

    private VerticalLayoutGroup layoutGroup;

    private void Start()
    {
        layoutGroup = GetComponent<VerticalLayoutGroup>();

        LoadLeaderboard();
    }

    private async void LoadLeaderboard()
    {
        loadingPanel.SetActive(true);

        PlayerRank[] ranks = await PlayerRankManager.Instance.DownloadScores();

        loadingPanel.SetActive(false);

        for (int i = 0; i < ranks.Length; i++)
        {
            UIPlayerScore score = Instantiate(playerScorePrefab, transform);
            (score.transform as RectTransform).sizeDelta = new Vector2(1, 60);

            score.Setup(ranks[i], i + 1);
        }

        RectTransform rect = transform as RectTransform;
        rect.offsetMin = new Vector2(rect.offsetMin.x, (transform.childCount * -(cellHeight + layoutGroup.spacing + layoutGroup.padding.bottom)) + rect.rect.height);
    }
}
