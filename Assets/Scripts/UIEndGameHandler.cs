using Cysharp.Threading.Tasks;
using System;
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

        if (Application.isPlaying)
        {
            Debug.Log("Returning to menu!");
            SceneManager.LoadScene(0);
        }
        else
        {
            Debug.Log("I'm not alive?");
        }
    }

    public async void LoadMenu()
    {
        if (loading) return;

        loading = true;

        GameManager.Instance.DisconnectServerRPC(playerUI.PlayerHandler.OwnerClientId);

        await UniTask.Delay(TimeSpan.FromSeconds(2));
        SceneManager.LoadScene(0);
    }
}
