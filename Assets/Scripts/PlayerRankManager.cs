using Cysharp.Threading.Tasks;
using Steamworks;
using System;
using System.IO;
using UnityEngine;

public class PlayerRankManager : Singleton<PlayerRankManager>
{
    private SteamLeaderboard leaderboard;

    private PlayerRank[] downloadedPlayerRanks;

    private int playerElo = 800;

    private bool downloading = false;

    private string rankSavePath;

    private async void Start()
    {
        await UniTask.WaitUntil(() => SteamManager.Initialized).Timeout(TimeSpan.FromSeconds(5));

        leaderboard = new SteamLeaderboard();
        leaderboard.Init();
        leaderboard.OnScoresDownloaded += Leaderboard_OnScoresDownloaded;
    }

    #region Saving & Loading

    private void OnEnable()
    {
        rankSavePath = Path.Combine(Application.dataPath, "PlayerRank");

        LoadRank();
    }

    private void OnDisable()
    {
        SaveRank();
    }

    public int GetRank()
    {
        if (playerElo == 800)
        {
            LoadRank();
        }

        return playerElo;
    }

    private void SaveRank()
    {
        using (var writer = new BinaryWriter(File.Open(rankSavePath, FileMode.OpenOrCreate)))
        {
            writer.Write(playerElo);
            Debug.Log("Succesfully saved elo " + playerElo);
        }
    }

    private void LoadRank()
    {
        if (!File.Exists(rankSavePath))
        {
            return;
        }

        using (var reader = new BinaryReader(File.Open(rankSavePath, FileMode.Open)))
        {
            int rank = reader.ReadInt32();
            playerElo = rank;

            Debug.Log("Loaded elo: " + playerElo);
        }
    }

    #endregion

    #region Callbacks

    public void WinGame(float opponentElo)
    {
        playerElo += 100;

        SaveRank();
        UpdateScore(playerElo);
    }

    public void LoseGame(float opponentElo)
    {
        playerElo -= 10;

        SaveRank();
        UpdateScore(playerElo);
    }

    #endregion

    #region Leaderboard

    private void Leaderboard_OnScoresDownloaded(PlayerRank[] scores)
    {
        downloadedPlayerRanks = scores;
    }

    private void Update()
    {
        leaderboard.UpdateCallbacks();
    }

    public void UpdateScore(int score)
    {
        leaderboard.UpdateScore(score);
    }

    public async UniTask<PlayerRank[]> DownloadScores()
    {
        if (leaderboard == null)
        {
            await UniTask.WaitUntil(() => leaderboard != null);
        }

        downloadedPlayerRanks = null;

        if (!downloading)
        {
            downloading = true;
            leaderboard.GetScores();
        }

        await UniTask.WaitUntil(() => downloadedPlayerRanks != null).Timeout(TimeSpan.FromSeconds(10));
        downloading = false;

        return downloadedPlayerRanks;
    }

    public int GetElo()
    {
        return GetRank();
    }
    #endregion
}

public class SteamLeaderboard
{
    public event Action<PlayerRank[]> OnScoresDownloaded;

    private const string LeaderboardName = "Ranked";
    private const ELeaderboardUploadScoreMethod LeaderboardMethod = ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest;

    private CallResult<LeaderboardFindResult_t> findResult = new CallResult<LeaderboardFindResult_t>();
    private CallResult<LeaderboardScoreUploaded_t> uploadResult = new CallResult<LeaderboardScoreUploaded_t>();
    private CallResult<LeaderboardScoresDownloaded_t> downloadResult = new CallResult<LeaderboardScoresDownloaded_t>();

    private SteamLeaderboard_t currentLeaderboard;

    public bool Initialized = false;

    public void Init()
    {
        SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(LeaderboardName);
        findResult.Set(hSteamAPICall, OnLeaderboardFindResult);
    }

    public void UpdateScore(int score)
    {
        if (!Initialized)
        {
            UnityEngine.Debug.Log("Can't upload to the leaderboard because isn't loadded yet");
        }
        else
        {
            UnityEngine.Debug.Log("uploading score(" + score + ") to steam leaderboard(" + LeaderboardName + ")");
            SteamAPICall_t steamAPICall = SteamUserStats.UploadLeaderboardScore(currentLeaderboard, LeaderboardMethod, score, null, 0);
            uploadResult.Set(steamAPICall, OnLeaderboardUploadResult);

        }
    }

    public void GetScores()
    {
        SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(currentLeaderboard, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, 100);
        downloadResult.Set(call, OnLeaderboardDownloadResult);
    }

    private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool failure)
    {
        UnityEngine.Debug.Log("STEAM LEADERBOARDS: Found - " + pCallback.m_bLeaderboardFound + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);
        currentLeaderboard = pCallback.m_hSteamLeaderboard;
        Initialized = true;
    }

    private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
    {
        UnityEngine.Debug.Log("STEAM LEADERBOARDS: failure - " + failure + " Completed - " + pCallback.m_bSuccess + " NewScore: " + pCallback.m_nGlobalRankNew + " Score " + pCallback.m_nScore + " HasChanged - " + pCallback.m_bScoreChanged);
    }

    private void OnLeaderboardDownloadResult(LeaderboardScoresDownloaded_t pCallback, bool failure)
    {
        UnityEngine.Debug.Log("STEAM LEADERBOARDS Download: Found - " + pCallback.m_hSteamLeaderboard + " leaderboardID - " + pCallback.m_hSteamLeaderboard.m_SteamLeaderboard);

        int count = pCallback.m_cEntryCount;
        PlayerRank[] ranks = new PlayerRank[count];
        for (int i = 0; i < count; i++)
        {
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out LeaderboardEntry_t entry, null, 0);

            ranks[i] = new PlayerRank
            {
                Elo = entry.m_nScore,
                User = new User(entry.m_steamIDUser)
            };
        }

        OnScoresDownloaded?.Invoke(ranks);
    }

    public void UpdateCallbacks()
    {
        SteamAPI.RunCallbacks();
    }
}

public class PlayerRank
{
    public User User;

    public string Username => User.SteamUsername;
    public int Elo;

}

public class User
{
    public event Action OnUsernameLoaded;
    public event Action OnAvatarLoaded;

    public CSteamID SteamID;
    public int AvatarID;

    public Texture2D SteamAvatarImage { get; private set; }

    public string SteamUsername { get; private set; }

    // Neccesary
    private Callback<PersonaStateChange_t> callPersona;
    private Callback<AvatarImageLoaded_t> callAvatar;
    private Callback<PersonaStateChange_t> personaState;

    public User(CSteamID id)
    {
        AvatarID = -1;
        SteamID = id;
        SteamUsername = SteamFriends.GetFriendPersonaName(id);

        if (SteamUsername == "" || SteamUsername == "[unknown]")
        {
            LoadName();
        }
        else
        {
            DownloadAvatar();
        }
    }

    private void LoadName()
    {
        personaState = Callback<PersonaStateChange_t>.Create((cb) =>
        {
            if (SteamID == (CSteamID)cb.m_ulSteamID)
            {
                SteamUsername = SteamFriends.GetFriendPersonaName(SteamID);
                if (SteamUsername == "" || SteamUsername == "[unknown]")
                {
                    LoadName();
                }
                else
                {
                    OnUsernameLoaded?.Invoke();
                    DownloadAvatar();
                }
            }
        });
    }

    private void DownloadAvatar()
    {
        Texture2D tex = GetUserAvatar(SteamID);
        if (tex != null)
        {
            SteamAvatarImage = tex;
            OnAvatarLoaded?.Invoke();
        }
    }

    private Texture2D GetUserAvatar(CSteamID id)
    {
        int handler = SteamFriends.GetLargeFriendAvatar(id);
        switch (handler)
        {
            case -1:
                callAvatar = Callback<AvatarImageLoaded_t>.Create((cb) =>
                {
                    if (id == cb.m_steamID)
                        AvatarLoaded(cb);
                });
                return SteamAvatarImage;
            case 0:
                if (SteamFriends.RequestUserInformation(id, false))
                {
                    callPersona = Callback<PersonaStateChange_t>.Create((cb) =>
                    {
                        if (id == (CSteamID)cb.m_ulSteamID)
                            PersonaStateChangeRequest(cb);
                    });
                    return SteamAvatarImage;
                }
                else
                    return GetTex(handler);
            default:
                return GetTex(handler);
        }
    }

    private Texture2D GetTex(int handler)
    {
        uint width, height;

        if (SteamUtils.GetImageSize(handler, out width, out height))
        {
            byte[] data = new byte[width * height * 4];
            if (SteamUtils.GetImageRGBA(handler, data, data.Length))
            {
                Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                tex.LoadRawTextureData(data);
                tex.Apply();
                return tex;
            }
        }
        return null;
    }

    private void PersonaStateChangeRequest(PersonaStateChange_t cb)
    {
        Texture2D tex = GetUserAvatar((CSteamID)cb.m_ulSteamID);
        if (tex != null)
        {
            SteamAvatarImage = tex;
            OnAvatarLoaded?.Invoke();
        }
    }

    private void AvatarLoaded(AvatarImageLoaded_t cb)
    {
        Texture2D tex = GetUserAvatar(cb.m_steamID);
        if (tex != null)
        {
            SteamAvatarImage = tex;
            OnAvatarLoaded?.Invoke();
        }
    }
}