using System;
using System.Collections.Generic;
using UnityEngine;

public class ReputationTierManager : MonoBehaviour
{
    public static ReputationTierManager Instance { get; private set; }

    [Header("Tier Data")]
    [SerializeField] private List<ReputationTierData> tiers = new();

    [Header("Reputation Requirement Formula")]
    [Tooltip("Reputacao necessaria para liberar o Tier 2.")]
    [SerializeField][Min(1)] private int firstExpansionRequirement = 100;

    [Tooltip("Crescimento linear dos requisitos apos o Tier 2.")]
    [SerializeField][Min(0)] private int linearGrowthPerTier = 80;

    [Tooltip("Crescimento quadratico dos requisitos apos o Tier 2. Mantem o inicio leve e exige mais no late game.")]
    [SerializeField][Min(0)] private int quadraticGrowthPerTier = 35;

    [Header("Fallbacks")]
    [SerializeField][Min(1)] private int fallbackRosterLimit = 3;
    [SerializeField] private bool clampReputationToCurrentTierCap = true;

    public event Action<ReputationTierData> OnCurrentTierChanged;
    public event Action<bool> OnExpansionAvailabilityChanged;
    public event Action<int> OnReputationCapChanged;

    private ResourceManager resourceManager;
    private RestaurantProgressionManager restaurantProgressionManager;
    private bool isExpansionAvailable;

    public ReputationTierData CurrentTier => GetTierByLevel(CurrentTierLevel);
    public ReputationTierData NextTier => GetTierByLevel(CurrentTierLevel + 1);

    public int CurrentTierLevel
    {
        get
        {
            if (restaurantProgressionManager != null)
                return Mathf.Max(1, restaurantProgressionManager.CurrentRestaurantLevel);

            if (RestaurantProgressionManager.Instance != null)
                return Mathf.Max(1, RestaurantProgressionManager.Instance.CurrentRestaurantLevel);

            return 1;
        }
    }

    public int CurrentRosterLimit => CurrentTier != null
        ? CurrentTier.MaxRosterSize
        : fallbackRosterLimit;

    public int CurrentTierMinimumReputation => GetRequiredReputationForTier(CurrentTier);

    public int CurrentReputationCap
    {
        get
        {
            ReputationTierData nextTier = NextTier;

            if (nextTier != null)
                return GetRequiredReputationForTier(nextTier);

            ReputationTierData currentTier = CurrentTier;

            if (currentTier != null)
                return GetRequiredReputationForTier(currentTier);

            return firstExpansionRequirement;
        }
    }

    public bool IsExpansionAvailable => isExpansionAvailable;
    public bool HasNextTier => NextTier != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SortTiers();
    }

    private void Start()
    {
        resourceManager = ResourceManager.Instance;
        restaurantProgressionManager = RestaurantProgressionManager.Instance;

        if (resourceManager != null)
            resourceManager.OnReputationChanged += HandleReputationChanged;

        if (restaurantProgressionManager != null)
            restaurantProgressionManager.OnRestaurantLevelChanged += HandleRestaurantLevelChanged;

        ClampReputationToCapIfNeeded();
        EvaluateExpansionAvailability(true);
        OnCurrentTierChanged?.Invoke(CurrentTier);
        OnReputationCapChanged?.Invoke(CurrentReputationCap);
    }

    private void OnDestroy()
    {
        if (resourceManager != null)
            resourceManager.OnReputationChanged -= HandleReputationChanged;

        if (restaurantProgressionManager != null)
            restaurantProgressionManager.OnRestaurantLevelChanged -= HandleRestaurantLevelChanged;

        if (Instance == this)
            Instance = null;
    }

    private void HandleReputationChanged(int newReputation)
    {
        EvaluateExpansionAvailability(false);
    }

    private void HandleRestaurantLevelChanged(int newRestaurantLevel)
    {
        ClampReputationToCapIfNeeded();
        EvaluateExpansionAvailability(true);
        OnCurrentTierChanged?.Invoke(CurrentTier);
        OnReputationCapChanged?.Invoke(CurrentReputationCap);

        // TODO: Quando houver assets de ambiente, aplicar troca visual/fisica de localidade aqui.
        // TODO: Quando loja/build system estiver pronto para tiers, aplicar desbloqueios de estruturas aqui.
    }

    public int GetRequiredReputationForTier(ReputationTierData tier)
    {
        if (tier == null)
            return 0;

        return tier.GetRequiredReputation(
            firstExpansionRequirement,
            linearGrowthPerTier,
            quadraticGrowthPerTier);
    }

    public int GetRequiredReputationForTierLevel(int tierLevel)
    {
        return GetRequiredReputationForTier(GetTierByLevel(tierLevel));
    }

    public ReputationTierData GetTierByLevel(int tierLevel)
    {
        if (tierLevel < 1)
            tierLevel = 1;

        for (int i = 0; i < tiers.Count; i++)
        {
            ReputationTierData tier = tiers[i];

            if (tier == null)
                continue;

            if (tier.TierLevel == tierLevel)
                return tier;
        }

        return null;
    }

    public bool CanConfirmTierExpansion()
    {
        if (!HasNextTier)
            return false;

        if (resourceManager == null)
            resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
            return false;

        return resourceManager.CurrentReputation >= CurrentReputationCap;
    }

    public bool TryConfirmTierExpansion()
    {
        if (!CanConfirmTierExpansion())
            return false;

        if (restaurantProgressionManager == null)
            restaurantProgressionManager = RestaurantProgressionManager.Instance;

        if (restaurantProgressionManager == null)
            return false;

        ReputationTierData nextTier = NextTier;

        if (nextTier == null)
            return false;

        bool changedLevel = restaurantProgressionManager.TrySetRestaurantLevel(nextTier.TierLevel);

        if (!changedLevel)
            return false;

        ApplyExpansionSideEffects(nextTier);
        return true;
    }

    private void ApplyExpansionSideEffects(ReputationTierData newTier)
    {
        if (newTier == null)
            return;

        // TODO: Trocar ambiente/localidade quando os assets existirem.
        // Exemplo futuro: EnvironmentManager.Instance.SetLocation(newTier.LocationId);

        // TODO: Desbloquear novas estruturas quando loja/build system estiver conectado a tiers.
        // Exemplo futuro: StructureUnlockManager.Instance.Unlock(newTier.StructureUnlockIds);
    }

    private void ClampReputationToCapIfNeeded()
    {
        if (!clampReputationToCurrentTierCap)
            return;

        if (resourceManager == null)
            resourceManager = ResourceManager.Instance;

        if (resourceManager == null)
            return;

        int cap = CurrentReputationCap;

        if (resourceManager.CurrentReputation > cap)
            resourceManager.SetReputation(cap);
    }

    private void EvaluateExpansionAvailability(bool forceNotify)
    {
        bool newAvailability = CanConfirmTierExpansion();

        if (!forceNotify && newAvailability == isExpansionAvailable)
            return;

        isExpansionAvailable = newAvailability;
        OnExpansionAvailabilityChanged?.Invoke(isExpansionAvailable);
    }

    private void SortTiers()
    {
        tiers.Sort((a, b) =>
        {
            if (a == null && b == null)
                return 0;

            if (a == null)
                return 1;

            if (b == null)
                return -1;

            return a.TierLevel.CompareTo(b.TierLevel);
        });
    }

    private void OnValidate()
    {
        firstExpansionRequirement = Mathf.Max(1, firstExpansionRequirement);
        linearGrowthPerTier = Mathf.Max(0, linearGrowthPerTier);
        quadraticGrowthPerTier = Mathf.Max(0, quadraticGrowthPerTier);
        fallbackRosterLimit = Mathf.Max(1, fallbackRosterLimit);
        SortTiers();
    }
}
