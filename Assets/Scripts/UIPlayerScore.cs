using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerScore : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI placeText;

    [SerializeField]
    private Image avatarImage;

    private PlayerRank playerRank;

    public void Setup(PlayerRank playerRank, int place)
    {
        this.playerRank = playerRank;

        if (!string.IsNullOrEmpty(playerRank.Username))
        {
            nameText.text = playerRank.Username;
            SetAvatar(playerRank.User.SteamAvatarImage);
        }

        scoreText.text = playerRank.Elo.ToString();
        placeText.text = string.Format("#{0}", place);

        playerRank.User.OnAvatarLoaded += User_OnAvatarLoaded;
        playerRank.User.OnUsernameLoaded += User_OnUsernameLoaded;
    }

    private void User_OnUsernameLoaded()
    {
        nameText.text = playerRank.User.SteamUsername;
    }

    private void User_OnAvatarLoaded()
    {
        SetAvatar(playerRank.User.SteamAvatarImage);
    }

    private void SetAvatar(Texture2D texture)
    {
        if (texture == null)
        {
            Debug.Log("STEAM AVATAR IS NULL");
            return;
        }

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(rect.width / 2, rect.height / 2));
        avatarImage.sprite = sprite;
    }
}
