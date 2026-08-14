using System.Collections.Generic;
using UnityEngine;

public class EmployeeRosterManager : MonoBehaviour
{
    private readonly List<EmployeeData> runtimeEmployeeCopies = new();
    public static EmployeeRosterManager Instance { get; private set; }

    [Header("Current Employee Roster")]
    [SerializeField] private List<EmployeeData> currentEmployees = new();
    [Header("Roster Limit")]
    [SerializeField, Min(1)] private int currentRosterLimit = 5;

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

        CreateRuntimeRoster();
    }

    private void Start()
    {
        reputationTierManager = ReputationTierManager.Instance;

        if (reputationTierManager != null)
            reputationTierManager.OnCurrentTierChanged += HandleCurrentTierChanged;

        if (EmployeeRuntimeManager.Instance != null)
            EmployeeRuntimeManager.Instance.SyncWithRoster(currentEmployees);

        RebuildAllViews();
    }

    private void OnDestroy()
    {
        if (reputationTierManager != null)
            reputationTierManager.OnCurrentTierChanged -= HandleCurrentTierChanged;

        for (int i = 0; i < runtimeEmployeeCopies.Count; i++)
        {
            EmployeeData runtimeEmployee = runtimeEmployeeCopies[i];

            if (runtimeEmployee != null)
                Destroy(runtimeEmployee);
        }

        runtimeEmployeeCopies.Clear();

        if (Instance == this)
            Instance = null;
    }

    private void HandleCurrentTierChanged(ReputationTierData currentTier)
    {
        RefreshAllViews();
    }

    public List<EmployeeData> GetCurrentEmployeesList()
    {
        return currentEmployees;
    }

    private void CreateRuntimeRoster()
    {
        List<EmployeeData> sourceEmployees =
            new List<EmployeeData>(currentEmployees);

        currentEmployees = new List<EmployeeData>();

        foreach (EmployeeData sourceEmployee in sourceEmployees)
        {
            if (sourceEmployee == null)
                continue;

            EmployeeData runtimeEmployee = Instantiate(sourceEmployee);

            runtimeEmployee.name = $"{sourceEmployee.name} (Runtime)";
            runtimeEmployee.hideFlags = HideFlags.DontSave;

            if (runtimeEmployee.ShouldResetRuntimeStateOnSessionStart())
                runtimeEmployee.ResetRuntimeStateForSession();

            runtimeEmployeeCopies.Add(runtimeEmployee);
            currentEmployees.Add(runtimeEmployee);
        }
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
            return Mathf.Max(1, ReputationTierManager.Instance.CurrentRosterLimit);

        return Mathf.Max(1, currentRosterLimit);
    }
}
