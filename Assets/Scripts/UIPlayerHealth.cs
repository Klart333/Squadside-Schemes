using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
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

    [SerializeField]
    private TextMeshProUGUI lostHealthText;

    [Title("Player")]
    [SerializeField]
    private Image playerProfile;

    [SerializeField]
    private Image deadPlayer;

    [SerializeField]
    private TextMeshProUGUI playerNameText;

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
                SetAvatar(user.SteamAvatarImage);

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
        playerNameText.text = user.SteamUsername;

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

    public async void DisplayLostHealth(int damage)
    {
        lostHealthText.gameObject.SetActive(true);
        lostHealthText.text = string.Format("-{0}", damage.ToString());

        await UniTask.Delay(TimeSpan.FromSeconds(2));

        lostHealthText.gameObject.SetActive(false);
    }
}
