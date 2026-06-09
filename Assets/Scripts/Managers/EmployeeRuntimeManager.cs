using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class EmployeeRuntimeManager : MonoBehaviour
{
    public static EmployeeRuntimeManager Instance { get; private set; }

    [Header("Employees To Manage")]
    [SerializeField] private List<EmployeeData> sessionEmployees = new();

    [Header("Stamina Recovery")]
    [SerializeField] private float passiveAvailableRecoveryPerSecond = 1f;
    [SerializeField] private float restingRecoveryPerSecond = 5f;

    private readonly HashSet<EmployeeData> uniqueEmployees = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildUniqueList();
        ResetEmployeesForSession();
    }

    private void Update()
    {
        if (ShouldPauseRecovery())
            return;

        TickRecovery();
    }

    private void BuildUniqueList()
    {
        uniqueEmployees.Clear();

        foreach (EmployeeData employee in sessionEmployees)
        {
            if (employee == null)
                continue;

            uniqueEmployees.Add(employee);
        }
    }

    public void RegisterEmployee(EmployeeData employee)
    {
        if (employee == null)
            return;

        uniqueEmployees.Add(employee);
    }

    public void RegisterEmployees(IReadOnlyList<EmployeeData> employees)
    {
        if (employees == null)
            return;

        for (int i = 0; i < employees.Count; i++)
            RegisterEmployee(employees[i]);
    }

    public void ResetEmployeesForSession()
    {
        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            if (employee.ShouldResetRuntimeStateOnSessionStart())
                employee.ResetRuntimeStateForSession();
        }
    }

    public void ResetEmployeesForNewDay()
    {
        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            employee.currentStamina = employee.maxStamina;
            employee.SetAvailable();
            employee.NotifyDataChanged();
        }
    }

    private bool ShouldPauseRecovery()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        return false;
    }

    private void TickRecovery()
    {
        foreach (EmployeeData employee in uniqueEmployees)
        {
            if (employee == null)
                continue;

            employee.TickPassiveRecovery(passiveAvailableRecoveryPerSecond, Time.deltaTime);
            employee.TickRestRecovery(restingRecoveryPerSecond, Time.deltaTime);
        }
    }
}