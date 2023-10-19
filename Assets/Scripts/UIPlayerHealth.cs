using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerHealth : MonoBehaviour
{
    [Title("Health")]
    [SerializeField]
    private TextMeshProUGUI healthText;

    [SerializeField]
    private Image healthFillImage;

    [Title("Player")]
    [SerializeField]
    private Image playerProfile;

    [SerializeField]
    private Image deadPlayer;

    private User user;

    private int startingHealth = 0;

    public int CurrentHealth { get; private set; }

    public void Setup(User steamUser, int startingHealth)
    {
        this.user = steamUser;

        this.startingHealth = startingHealth;

        healthText.text = startingHealth.ToString();
        healthFillImage.fillAmount = 1.0f;

        if (steamUser != null)
        {
            if (steamUser.SteamAvatarImage != null)
            {
                SetAvatar(steamUser.SteamAvatarImage);
            }
            else
            {
                steamUser.OnAvatarLoaded += SteamUser_OnAvatarLoaded;
            }
        }

        CurrentHealth = startingHealth;
    }

    private void SteamUser_OnAvatarLoaded()
    {
        SetAvatar(user.SteamAvatarImage);
    }

    private void SetAvatar(Texture2D texture)
    {
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(rect.width / 2, rect.height / 2));
        playerProfile.sprite = sprite;
    }

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
        healthFillImage.fillAmount = (float)health / startingHealth;

        CurrentHealth = health;
    }

    public void SetPlayerDead()
    {
        CurrentHealth = 0;

        healthText.text = "0";
        healthFillImage.fillAmount = 0;
        deadPlayer.enabled = true;
    }
}
