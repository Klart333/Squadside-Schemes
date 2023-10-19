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

    public void EndGame(bool won)
    {
        gameObject.SetActive(true);

        if (won)
        {
            resultText.text = winText;
        }
        else
        {
            resultText.text = loseText;
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);

        NetworkManager.Singleton.DisconnectClient(playerUI.PlayerHandler.OwnerClientId);
    }
}
