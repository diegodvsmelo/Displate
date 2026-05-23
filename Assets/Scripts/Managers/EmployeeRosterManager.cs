using System.Collections.Generic;
using UnityEngine;

public class EmployeeRosterManager : MonoBehaviour
{
    public static EmployeeRosterManager Instance { get; private set; }

    [Header("Current Employee Roster")]
    [SerializeField] private List<EmployeeData> currentEmployees = new();

    [Header("UI Views")]
    [SerializeField] private StaffSidebarUI compactSidebarUI;
    [SerializeField] private EmployeeCardListUI expandedSidebarUI;

    public IReadOnlyList<EmployeeData> CurrentEmployees => currentEmployees;
    public int CurrentRosterLimit => GetCurrentRosterLimit();
    public bool HasRosterSlotAvailable => currentEmployees.Count < CurrentRosterLimit;

    private ReputationTierManager reputationTierManager;

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
        reputationTierManager = ReputationTierManager.Instance;

        if (reputationTierManager != null)
            reputationTierManager.OnCurrentTierChanged += HandleCurrentTierChanged;

        RebuildAllViews();
    }

    private void OnDestroy()
    {
        if (reputationTierManager != null)
            reputationTierManager.OnCurrentTierChanged -= HandleCurrentTierChanged;
    }

    private void HandleCurrentTierChanged(ReputationTierData currentTier)
    {
        RefreshAllViews();
    }

    public List<EmployeeData> GetCurrentEmployeesList()
    {
        return currentEmployees;
    }

    public void SetEmployees(List<EmployeeData> employees)
    {
        currentEmployees = employees != null
            ? new List<EmployeeData>(employees)
            : new List<EmployeeData>();

        RebuildAllViews();
    }

    public void AddEmployee(EmployeeData employee)
    {
        TryAddEmployee(employee);
    }

    public bool TryAddEmployee(EmployeeData employee)
    {
        if (employee == null)
            return false;

        if (currentEmployees.Contains(employee))
            return false;

        if (!HasRosterSlotAvailable)
        {
            Debug.LogWarning($"[EmployeeRosterManager] Roster cheio. Limite atual: {CurrentRosterLimit}.");
            return false;
        }

        currentEmployees.Add(employee);
        RebuildAllViews();
        return true;
    }

    public bool CanAddEmployee(EmployeeData employee)
    {
        if (employee == null)
            return false;

        if (currentEmployees.Contains(employee))
            return false;

        return HasRosterSlotAvailable;
    }

    public void RemoveEmployee(EmployeeData employee)
    {
        if (employee == null)
            return;

        if (currentEmployees.Remove(employee))
            RebuildAllViews();
    }

    public void RebuildAllViews()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Rebuild(currentEmployees);

        if (expandedSidebarUI != null)
            expandedSidebarUI.Rebuild(currentEmployees);
    }

    public void RefreshAllViews()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Refresh();

        if (expandedSidebarUI != null)
            expandedSidebarUI.Refresh();
    }

    public void RebuildCompactView()
    {
        if (compactSidebarUI != null)
            compactSidebarUI.Rebuild(currentEmployees);
    }

    public void RebuildExpandedView()
    {
        if (expandedSidebarUI != null)
            expandedSidebarUI.Rebuild(currentEmployees);
    }

    private int GetCurrentRosterLimit()
    {
        if (ReputationTierManager.Instance != null)
            return ReputationTierManager.Instance.CurrentRosterLimit;

        return int.MaxValue;
    }
}
