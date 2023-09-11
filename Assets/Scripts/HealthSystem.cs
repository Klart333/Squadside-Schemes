using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public event Action OnHealthChanged;

    [Title("Health")]
    [SerializeField]
    private int startingHealth = 100;

    public int StartingHealth => startingHealth;
    public PlayerHandler PlayerHandler { get; set; }
    public int CurrentHealth { get; private set; }

    private void Start()
    {
        CurrentHealth = startingHealth;
    }

    public void LoseHealth(int amount)
    {
        CurrentHealth -= amount;
        OnHealthChanged?.Invoke();
    }

    public void GainHealth(int amount)
    {
        CurrentHealth += amount;
        OnHealthChanged?.Invoke();
    }
}