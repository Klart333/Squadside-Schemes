using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;

public class UITypeOutText : MonoBehaviour
{
    [SerializeField]
    private float lettersPerSecond;

    private TextMeshProUGUI text;

    private string typeString;
    private float textSize;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        typeString = text.text;

        textSize = text.fontSize;
        text.enableAutoSizing = false;
        text.fontSize = textSize;

        text.text = "";
    }

    public async void Start()
    {
        for (int i = 0; i < typeString.Length; i++)
        {
            text.text += typeString[i];
            await UniTask.Delay(TimeSpan.FromSeconds(1.0f / lettersPerSecond));
        }
    }
}
