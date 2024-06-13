public class PlayerCountManager : Singleton<PlayerCountManager>
{
    public Highscores Highscores { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        DontDestroyOnLoad(this.gameObject);

        Highscores = new Highscores(this);

        Highscores.AddNewHighscore("0", 0);
        Highscores.AddNewHighscore("1", 0);
    }

    public void OnJoinQueue()
    {
        StartCoroutine(Highscores.UpdatePlayerCount(0, 1));
    }

    public void OnLeaveQueue()
    {
        StartCoroutine(Highscores.UpdatePlayerCount(0, -1));
    }

    public void OnJoinGame()
    {
        StartCoroutine(Highscores.UpdatePlayerCount(0, -2));

        StartCoroutine(Highscores.UpdatePlayerCount(1, 2));
    }

    public void OnLeaveGame()
    {
        StartCoroutine(Highscores.UpdatePlayerCount(1, -2));
    }
}
