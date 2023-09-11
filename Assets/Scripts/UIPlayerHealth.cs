using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
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

    private int startingHealth = 0;

    public int CurrentHealth { get; private set; }

    public void Setup(Sprite playerProfileSprite, int startingHealth)
    {
        this.startingHealth = startingHealth;

        healthText.text = startingHealth.ToString();
        healthFillImage.fillAmount = 1.0f;

        if (playerProfileSprite)
        {
            playerProfile.sprite = playerProfileSprite;
        }

        CurrentHealth = startingHealth;
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
