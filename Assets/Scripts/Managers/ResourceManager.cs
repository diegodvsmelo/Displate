using UnityEngine;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [Header("Starting Values")]
    [SerializeField] private int currentMoney = 100;
    [SerializeField] private int currentReputation = 50;

    public event Action<int> OnMoneyChanged;
    public event Action<int> OnReputationChanged;
    public event Action OnMoneyInsufficient;
    public event Action OnReputationReachedZero;

    public int CurrentMoney => currentMoney;
    public int CurrentReputation => currentReputation;
    public int CurrentReputationCap => GetCurrentReputationCap();
    public bool IsReputationAtCurrentCap => currentReputation >= CurrentReputationCap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ClampReputationToCurrentCap();
        NotifyAll();
    }

    public void ModifyMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool TrySpendMoney(int amount)
    {
        if (currentMoney < amount)
        {
            OnMoneyInsufficient?.Invoke();
            return false;
        }

        ModifyMoney(-amount);
        return true;
    }

    public void ModifyReputation(int amount)
    {
        currentReputation += amount;
        ClampReputationToCurrentCap();

        OnReputationChanged?.Invoke(currentReputation);

        if (currentReputation <= 0)
            OnReputationReachedZero?.Invoke();
    }

    public void SetMoney(int value)
    {
        currentMoney = Mathf.Max(0, value);
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public void SetReputation(int value)
    {
        currentReputation = value;
        ClampReputationToCurrentCap();

        OnReputationChanged?.Invoke(currentReputation);

        if (currentReputation <= 0)
            OnReputationReachedZero?.Invoke();
    }

    private void ClampReputationToCurrentCap()
    {
        currentReputation = Mathf.Clamp(currentReputation, 0, GetCurrentReputationCap());
    }

    private int GetCurrentReputationCap()
    {
        if (ReputationTierManager.Instance != null)
            return Mathf.Max(0, ReputationTierManager.Instance.CurrentReputationCap);

        return int.MaxValue;
    }

    private void NotifyAll()
    {
        OnMoneyChanged?.Invoke(currentMoney);
        OnReputationChanged?.Invoke(currentReputation);
    }
}
