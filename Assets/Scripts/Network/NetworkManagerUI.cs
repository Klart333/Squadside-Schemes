using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] TextMeshProUGUI textUpper;
    [SerializeField] TextMeshProUGUI textLower;


    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });

        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
    }
    public void SetTexts(string stringOne, string stringTwo)
    {
        textUpper.text = $"Target: {stringOne}";
        textLower.text = $"Ally Target: {stringTwo}";
    }
}