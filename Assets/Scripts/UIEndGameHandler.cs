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

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);

        NetworkManager.Singleton.DisconnectClient(playerUI.PlayerHandler.OwnerClientId);
    }
}
