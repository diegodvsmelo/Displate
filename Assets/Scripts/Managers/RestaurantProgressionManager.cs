using System;
using UnityEngine;

public class RestaurantProgressionManager : MonoBehaviour
{
    public static RestaurantProgressionManager Instance { get; private set; }

    [Header("Restaurant Level")]
    [SerializeField] private int currentRestaurantLevel = 1;
    [SerializeField] private int maxRestaurantLevel = 5;

    public event Action<int> OnRestaurantLevelChanged;

    public int CurrentRestaurantLevel => currentRestaurantLevel;
    public int MaxRestaurantLevel => maxRestaurantLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        maxRestaurantLevel = Mathf.Max(1, maxRestaurantLevel);
        currentRestaurantLevel = Mathf.Clamp(currentRestaurantLevel, 1, maxRestaurantLevel);
    }

    public void SetRestaurantLevel(int newLevel)
    {
        TrySetRestaurantLevel(newLevel);
    }

    public bool TrySetRestaurantLevel(int newLevel)
    {
        int clampedLevel = Mathf.Clamp(newLevel, 1, maxRestaurantLevel);

        if (clampedLevel == currentRestaurantLevel)
            return false;

        currentRestaurantLevel = clampedLevel;
        OnRestaurantLevelChanged?.Invoke(currentRestaurantLevel);
        return true;
    }

    public void IncreaseRestaurantLevel(int amount = 1)
    {
        if (amount <= 0)
            return;

        SetRestaurantLevel(currentRestaurantLevel + amount);
    }

    public void DecreaseRestaurantLevel(int amount = 1)
    {
        if (amount <= 0)
            return;

        SetRestaurantLevel(currentRestaurantLevel - amount);
    }

    public bool IsAtMaxLevel()
    {
        return currentRestaurantLevel >= maxRestaurantLevel;
    }
}
