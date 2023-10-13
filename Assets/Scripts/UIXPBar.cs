using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIXPBar : MonoBehaviour
{
    [SerializeField]
    private PlayerUI playerUI;

    [SerializeField]
    private GameObject barPrefab;

    [SerializeField]
    private TextMeshProUGUI currentXPText;

    private int smalBar = 0;

    private void Start()
    {
        playerUI.PlayerHandler.LevelSystem.OnXPChanged += LevelSystem_OnXPChanged;
        playerUI.PlayerHandler.LevelSystem.OnLevelUp += LevelSystem_OnLevelUp;
    }

    private void LevelSystem_OnLevelUp()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        int total = playerUI.PlayerHandler.LevelSystem.xpThresholds[playerUI.PlayerHandler.LevelSystem.CurrentLevel - 1];
        smalBar = total % 4 == 0 ? 0 : 1;
        int totalBars = Mathf.FloorToInt(total / 4.0f);

        if (smalBar > 0)
        {
            var gm = Instantiate(barPrefab, transform);
            gm.transform.localScale = new Vector3(0.5f, 1, 1);
        }

        for (int i = 0; i < totalBars; i++)
        {
            Instantiate(barPrefab, transform);
        }
    }

    private async void LevelSystem_OnXPChanged()
    {
        await UniTask.NextFrame();

        int total = playerUI.PlayerHandler.LevelSystem.xpThresholds[playerUI.PlayerHandler.LevelSystem.CurrentLevel - 1];

        currentXPText.text = string.Format("<color=lightblue>{0}</color>/{1}", playerUI.PlayerHandler.LevelSystem.CurrentXP, total);

        int xp = playerUI.PlayerHandler.LevelSystem.CurrentXP;
        if (smalBar > 0)
        {
            float amount = (float)xp / 2.0f;
            transform.GetChild(0).GetChild(0).GetComponent<Image>().fillAmount = amount;

            xp -= 2;
        }


        for (int i = smalBar; i < transform.childCount; i++)
        {
            float amount = (float)xp / 4.0f;
            transform.GetChild(i).GetChild(0).GetComponent<Image>().fillAmount = amount;

            xp -= 4;
        }
    }
}
