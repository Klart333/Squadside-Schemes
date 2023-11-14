using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEndGameHandler : MonoBehaviour
{
    [SerializeField]
    private PlayerUI playerUI;

    [SerializeField]
    private TextMeshProUGUI resultText;

    [SerializeField]
    [TextArea]
    private string winText = "you won!";

    [SerializeField]
    [TextArea]
    private string loseText = "ya lost";

    private bool loading = false;

    private void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
    }

    public void EndGame(bool lost)
    {
        gameObject.SetActive(true);

        if (lost)
        {
            resultText.text = loseText;
        }
        else
        {
            resultText.text = winText;
        }
    }

    private void Singleton_OnClientDisconnectCallback(ulong obj)
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(0);
    }

    public void LoadMenu()
    {
        if (loading) return;

        loading = true;

        GameManager.Instance.DisconnectServerRPC(playerUI.PlayerHandler.OwnerClientId);

    }
}
