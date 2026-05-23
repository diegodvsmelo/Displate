using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Reputation Tier", menuName = "Restaurant/Progression/Reputation Tier")]
public class ReputationTierData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField][Min(1)] private int tierLevel = 1;
    [SerializeField] private string tierName = "New Tier";

    [Header("Reputation Requirement")]
    [Tooltip("Tier 1 sempre deve exigir 0. Use override apenas se quiser fugir da formula global do ReputationTierManager.")]
    [SerializeField] private bool useCustomRequiredReputation = false;
    [SerializeField][Min(0)] private int customRequiredReputation = 0;

    [Header("Roster Limit")]
    [SerializeField][Min(1)] private int maxRosterSize = 3;

    [Header("Future Location/Structure Hooks")]
    [Tooltip("Reservado para quando houver assets de ambientes/localidades.")]
    [SerializeField] private string locationId = "";

    [Tooltip("Reservado para quando loja/desbloqueio de estruturas for conectado a tiers.")]
    [SerializeField] private List<string> structureUnlockIds = new();

    public int TierLevel => Mathf.Max(1, tierLevel);
    public string TierName => tierName;
    public bool UseCustomRequiredReputation => useCustomRequiredReputation;
    public int CustomRequiredReputation => Mathf.Max(0, customRequiredReputation);
    public int MaxRosterSize => Mathf.Max(1, maxRosterSize);
    public string LocationId => locationId;
    public IReadOnlyList<string> StructureUnlockIds => structureUnlockIds;

    public int GetRequiredReputation(int firstExpansionRequirement, int linearGrowthPerTier, int quadraticGrowthPerTier)
    {
        if (TierLevel <= 1)
            return 0;

        if (useCustomRequiredReputation)
            return CustomRequiredReputation;

        int stepsAfterFirstExpansion = TierLevel - 2;

        return Mathf.Max(0,
            firstExpansionRequirement
            + (linearGrowthPerTier * stepsAfterFirstExpansion)
            + (quadraticGrowthPerTier * stepsAfterFirstExpansion * stepsAfterFirstExpansion));
    }

    private void OnValidate()
    {
        tierLevel = Mathf.Max(1, tierLevel);
        customRequiredReputation = Mathf.Max(0, customRequiredReputation);
        maxRosterSize = Mathf.Max(1, maxRosterSize);

        if (tierLevel <= 1)
        {
            useCustomRequiredReputation = true;
            customRequiredReputation = 0;
        }
    }
}
