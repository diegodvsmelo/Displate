using System;
using UnityEngine;

public enum EmployeeAvailabilityState
{
    Available,
    Occupied,
    Resting
}

[CreateAssetMenu(fileName = "New Employee", menuName = "Restaurant/Employee")]
public class EmployeeData : ScriptableObject
{
    public event Action OnDataChanged;

    [Header("Identity")]
    public string employeeName;
    public Sprite profilePicture;

    [Header("Skills (0-10)")]
    [Range(0, 10)] public int cookingSkill;
    [Range(0, 10)] public int serviceSkill;
    [Range(0, 10)] public int operationalSkill;

    [Header("Physical Stats")]
    [Range(0, 10)] public int agility;
    [Min(1)] public int maxStamina = 100;
    [Min(0)] public int currentStamina = 100;

    [Header("Availability")]
    public EmployeeAvailabilityState availabilityState = EmployeeAvailabilityState.Available;

    [Header("Progression")]
    [Min(1)] public int currentLevel = 1;
    [Min(0)] public int currentXP = 0;
    [Min(0)] public int skillPoints = 0;

    [Header("Trait")]
    public bool hasTrait;
    public string traitName;

    [Header("Status Icons")]
    [Tooltip("Pode ser buff, debuff ou qualquer outro status visual.")]
    public Sprite statusIconA;

    [Tooltip("Pode ser buff, debuff ou qualquer outro status visual.")]
    public Sprite statusIconB;

    [Header("Contract Info")]
    [Min(0)] public int baseSalary = 1;

    [Header("Session Defaults")]
    [SerializeField] private bool resetRuntimeStateOnSessionStart = true;
    [SerializeField] private bool startSessionWithFullStamina = true;
    [SerializeField][Min(0)] private int sessionStartStamina = 100;
    [SerializeField][Min(1)] private int sessionStartLevel = 1;
    [SerializeField][Min(0)] private int sessionStartXP = 0;
    [SerializeField][Min(0)] private int sessionStartSkillPoints = 0;
    [SerializeField] private EmployeeAvailabilityState sessionStartAvailability = EmployeeAvailabilityState.Available;

    [NonSerialized] private float restingRecoveryBuffer;
    [NonSerialized] private float passiveRecoveryBuffer;

    private void OnValidate()
    {
        if (maxStamina < 1)
            maxStamina = 1;

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentLevel < 1)
            currentLevel = 1;

        if (currentXP < 0)
            currentXP = 0;

        if (skillPoints < 0)
            skillPoints = 0;

        if (baseSalary < 0)
            baseSalary = 0;

        if (sessionStartStamina < 0)
            sessionStartStamina = 0;

        sessionStartStamina = Mathf.Clamp(sessionStartStamina, 0, maxStamina);

        if (sessionStartLevel < 1)
            sessionStartLevel = 1;

        if (sessionStartXP < 0)
            sessionStartXP = 0;

        if (sessionStartSkillPoints < 0)
            sessionStartSkillPoints = 0;

        if (!hasTrait)
            traitName = "";

        if (currentStamina <= 0 && availabilityState == EmployeeAvailabilityState.Available)
            availabilityState = EmployeeAvailabilityState.Resting;
    }

    public bool ShouldResetRuntimeStateOnSessionStart()
    {
        return resetRuntimeStateOnSessionStart;
    }

    public void ResetRuntimeStateForSession()
    {
        currentLevel = Mathf.Max(1, sessionStartLevel);
        currentXP = Mathf.Max(0, sessionStartXP);
        skillPoints = Mathf.Max(0, sessionStartSkillPoints);

        currentStamina = startSessionWithFullStamina
            ? maxStamina
            : Mathf.Clamp(sessionStartStamina, 0, maxStamina);

        availabilityState = currentStamina <= 0
            ? EmployeeAvailabilityState.Resting
            : sessionStartAvailability;

        if (availabilityState == EmployeeAvailabilityState.Occupied)
            availabilityState = EmployeeAvailabilityState.Available;

        restingRecoveryBuffer = 0f;
        passiveRecoveryBuffer = 0f;

        NotifyDataChanged();
    }

    public bool HasUnspentSkillPoints()
    {
        return skillPoints > 0;
    }

    public bool HasTrait()
    {
        return hasTrait && !string.IsNullOrWhiteSpace(traitName);
    }

    public bool HasStatusA()
    {
        return statusIconA != null;
    }

    public bool HasStatusB()
    {
        return statusIconB != null;
    }

    public int GetDailyCost()
    {
        int restaurantLevel = 1;

        if (RestaurantProgressionManager.Instance != null)
            restaurantLevel = RestaurantProgressionManager.Instance.CurrentRestaurantLevel;

        return GetDailyCost(restaurantLevel);
    }

    public int GetDailyCost(int restaurantLevel)
    {
        int safeEmployeeLevel = Mathf.Max(1, currentLevel);
        int safeRestaurantLevel = Mathf.Max(1, restaurantLevel);

        return baseSalary
               + (2 * (safeEmployeeLevel - 1))
               + (2 * (safeRestaurantLevel - 1));
    }

    public int GetXpToNextLevel()
    {
        return currentLevel * 100;
    }

    public float GetStaminaPercent()
    {
        if (maxStamina <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentStamina / maxStamina);
    }

    public float GetXpPercent()
    {
        int xpToNextLevel = GetXpToNextLevel();

        if (xpToNextLevel <= 0)
            return 0f;

        return Mathf.Clamp01((float)currentXP / xpToNextLevel);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        currentXP += amount;

        while (currentXP >= GetXpToNextLevel())
        {
            currentXP -= GetXpToNextLevel();
            currentLevel++;
            skillPoints++;
        }

        currentXP = Mathf.Max(0, currentXP);
        NotifyDataChanged();
    }

    public void SetOccupied()
    {
        if (availabilityState == EmployeeAvailabilityState.Occupied)
            return;

        availabilityState = EmployeeAvailabilityState.Occupied;
        NotifyDataChanged();
    }

    public void SetAvailable()
    {
        if (currentStamina <= 0)
        {
            SetResting();
            return;
        }

        if (availabilityState == EmployeeAvailabilityState.Available)
            return;

        availabilityState = EmployeeAvailabilityState.Available;
        NotifyDataChanged();
    }

    public void SetResting()
    {
        if (availabilityState == EmployeeAvailabilityState.Resting)
            return;

        availabilityState = EmployeeAvailabilityState.Resting;
        restingRecoveryBuffer = 0f;
        NotifyDataChanged();
    }

    public bool CanBeAssignedToTask()
    {
        return availabilityState == EmployeeAvailabilityState.Available && currentStamina > 0;
    }

    public bool IsAvailable()
    {
        return availabilityState == EmployeeAvailabilityState.Available;
    }

    public bool IsOccupied()
    {
        return availabilityState == EmployeeAvailabilityState.Occupied;
    }

    public bool IsResting()
    {
        return availabilityState == EmployeeAvailabilityState.Resting;
    }

    public void ConsumeStamina(int amount)
    {
        if (amount <= 0)
            return;

        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentStamina <= 0)
            SetResting();
        else
            NotifyDataChanged();
    }

    public void RecoverStamina(int amount)
    {
        if (amount <= 0)
            return;

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        if (currentStamina >= maxStamina && availabilityState == EmployeeAvailabilityState.Resting)
            availabilityState = EmployeeAvailabilityState.Available;

        NotifyDataChanged();
    }

    public void TickPassiveRecovery(float staminaPerSecond, float deltaTime)
    {
        if (!IsAvailable())
            return;

        if (staminaPerSecond <= 0f || deltaTime <= 0f)
            return;

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            return;
        }

        passiveRecoveryBuffer += staminaPerSecond * deltaTime;

        int recoveredWholePoints = Mathf.FloorToInt(passiveRecoveryBuffer);

        if (recoveredWholePoints <= 0)
            return;

        passiveRecoveryBuffer -= recoveredWholePoints;
        currentStamina = Mathf.Clamp(currentStamina + recoveredWholePoints, 0, maxStamina);

        NotifyDataChanged();
    }

    public void TickRestRecovery(float staminaPerSecond, float deltaTime)
    {
        if (!IsResting())
            return;

        if (staminaPerSecond <= 0f || deltaTime <= 0f)
            return;

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            SetAvailable();
            return;
        }

        restingRecoveryBuffer += staminaPerSecond * deltaTime;

        int recoveredWholePoints = Mathf.FloorToInt(restingRecoveryBuffer);

        if (recoveredWholePoints <= 0)
            return;

        restingRecoveryBuffer -= recoveredWholePoints;
        currentStamina = Mathf.Clamp(currentStamina + recoveredWholePoints, 0, maxStamina);

        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
            SetAvailable();
            return;
        }

        NotifyDataChanged();
    }

    public void NotifyDataChanged()
    {
        OnDataChanged?.Invoke();
    }
}