using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaffRecruitmentManager : MonoBehaviour
{
    public static StaffRecruitmentManager Instance { get; private set; }

    [Header("Window Cadence")]
    [SerializeField, Min(1)] private int recruitmentIntervalDays = 3;

    [Header("Recruitment Pool")]
    [SerializeField] private List<EmployeeData> candidatePool = new();
    [SerializeField, Min(1)] private int recruitsPerWindow = 3;

    [Tooltip("Se ativo, gera novos funcionários em runtime. Se desativado, usa Candidate Pool.")]
    [SerializeField] private bool randomizeRecruits = true;

    [Header("Generated Recruits - Identity")]
    [SerializeField]
    private List<string> recruitNamePool = new()
    {
        "Vitor",
        "Lucas",
        "Junior",
        "Gabriel",
        "Miguel",
        "Manuel",
        "Davi",
        "Julia",
        "Laura",
        "Alice",
        "Henrique",
        "Lais",
        "Kiev",
        "Madeiral",
        "Mirella",
        "Deco"
    };

    [SerializeField] private List<Sprite> recruitPortraitPool = new();

    [Tooltip("Permite que um funcionário gerado venha sem imagem.")]
    [SerializeField] private bool allowEmptyPortrait = true;

    [Range(0f, 1f)]
    [SerializeField] private float emptyPortraitChance = 0.35f;

    [Header("Generated Recruits - Prestige Scaling")]
    [SerializeField, Min(1)] private int minPrestigeLevelForRecruitScaling = 1;
    [SerializeField, Min(1)] private int maxPrestigeLevelForRecruitScaling = 5;

    [SerializeField, Range(0, 10)] private int minGeneratedSkill = 1;
    [SerializeField, Range(0, 10)] private int maxGeneratedSkill = 10;

    [Tooltip("Quanto maior, mais o early game tende a gerar atributos baixos.")]
    [SerializeField, Min(0.1f)] private float lowPrestigeSkillPower = 2.25f;

    [Tooltip("Quanto menor que 1, mais o late game tende a gerar atributos altos.")]
    [SerializeField, Min(0.1f)] private float highPrestigeSkillPower = 0.55f;

    
    [Header("Generated Recruits - Stamina")]
    [SerializeField, Min(1)] private int minGeneratedMaxStamina = 75;

    [Tooltip("Valor de referencia/padrao esperado no começo do jogo.")]
    [SerializeField, Min(1)] private int defaultGeneratedMaxStamina = 100;

    [SerializeField, Min(1)] private int maxGeneratedMaxStamina = 200;

    [Tooltip("Quanto maior, mais o early game tende a gerar stamina baixa/proxima do valor padrao.")]
    [SerializeField, Min(0.1f)] private float lowPrestigeStaminaPower = 4f;

    [Tooltip("Quanto menor que 1, mais o late game tende a gerar stamina alta.")]
    [SerializeField, Min(0.1f)] private float highPrestigeStaminaPower = 0.65f;

    [Header("Generated Recruits - Contract")]
    [SerializeField, Min(1)] private int generatedStartingLevel = 1;
    [SerializeField, Min(0)] private int minGeneratedBaseSalary = 1;
    [SerializeField, Min(0)] private int maxGeneratedBaseSalary = 8;

    [Header("UI Root")]
    [SerializeField] private GameObject screenRoot;
    [Header("External UI")]
    [SerializeField] private GameObject sidebarEmployeeRoot;

    [Header("Roster UI")]
    [SerializeField] private Transform rosterSlotsContainer;
    [SerializeField] private RecruitmentRosterSlotUI rosterSlotPrefab;
    [SerializeField] private TextMeshProUGUI currentStaffCountText;

    [Header("Recruit UI")]
    [SerializeField] private Transform recruitsContainer;
    [SerializeField] private TextMeshProUGUI availableRecruitsCountText;

    [Header("Card Prefab")]
    [SerializeField] private RecruitmentEmployeeCardUI employeeCardPrefab;

    [Header("Summary")]
    [SerializeField] private TextMeshProUGUI payrollText;
    [SerializeField] private RecruitmentPayrollIndicatorUI payrollIndicatorUI;

    [Header("Buttons")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Rules")]
    [SerializeField, Min(0)] private int minimumRosterSize = 1;
    [SerializeField, Min(1)] private int fallbackRosterLimit = 5;
    [SerializeField] private bool chargeHiringCostsOnConfirm = false;

    private readonly List<EmployeeData> originalRoster = new();
    private readonly List<EmployeeData> workingRoster = new();
    private readonly List<EmployeeData> currentRecruitOptions = new();
    private int generatedRecruitSerial = 0;
    private int originalPayrollAtWindowOpen;

    private readonly List<RecruitmentRosterSlotUI> spawnedSlots = new();
    private readonly List<RecruitmentEmployeeCardUI> spawnedCards = new();
    private readonly Dictionary<EmployeeData, RecruitmentEmployeeCardUI> cardByEmployee = new();

    private Action onRecruitmentFinished;

    private bool sidebarEmployeeWasActive;
    private bool hasStoredSidebarEmployeeState;

    public int RecruitmentIntervalDays => recruitmentIntervalDays;
    public bool IsRecruitmentOpen { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (screenRoot != null)
            screenRoot.SetActive(false);

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmChanges);
            confirmButton.onClick.AddListener(ConfirmChanges);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelChanges);
            cancelButton.onClick.AddListener(CancelChanges);
        }
    }

    private void HideSidebarEmployee()
    {
        if (sidebarEmployeeRoot == null)
            return;

        sidebarEmployeeWasActive = sidebarEmployeeRoot.activeSelf;
        hasStoredSidebarEmployeeState = true;

        sidebarEmployeeRoot.SetActive(false);
    }

    private void RestoreSidebarEmployee()
    {
        if (sidebarEmployeeRoot == null)
            return;

        if (!hasStoredSidebarEmployeeState)
            return;

        sidebarEmployeeRoot.SetActive(sidebarEmployeeWasActive);

        hasStoredSidebarEmployeeState = false;
    }

    public bool ShouldOpenAfterCompletedDay(int completedDay)
    {
        if (completedDay <= 0)
            return false;

        if (recruitmentIntervalDays <= 0)
            return false;

        return completedDay % recruitmentIntervalDays == 0;
    }

    public void OpenRecruitment(Action onFinished)
    {
        IsRecruitmentOpen = true;
        onRecruitmentFinished = onFinished;

        HideSidebarEmployee();

        BuildTemporaryState();
        BuildUI();

        if (screenRoot != null)
            screenRoot.SetActive(true);

        RefreshAllVisuals();
    }

    private void BuildTemporaryState()
    {
        originalRoster.Clear();
        workingRoster.Clear();
        currentRecruitOptions.Clear();

        if (EmployeeRosterManager.Instance != null)
        {
            IReadOnlyList<EmployeeData> currentEmployees = EmployeeRosterManager.Instance.CurrentEmployees;

            for (int i = 0; i < currentEmployees.Count; i++)
            {
                EmployeeData employee = currentEmployees[i];

                if (employee == null)
                    continue;

                originalRoster.Add(employee);
                workingRoster.Add(employee);
            }
        }

        originalPayrollAtWindowOpen = CalculatePayroll(originalRoster);

        BuildRecruitOptions();
    }

    
    private void BuildRecruitOptions()
    {
        currentRecruitOptions.Clear();

        if (randomizeRecruits)
        {
            GenerateRandomRecruitOptions();
            return;
        }

        BuildRecruitOptionsFromCandidatePool();
    }

    private void BuildRecruitOptionsFromCandidatePool()
    {
        List<EmployeeData> validCandidates = new();

        for (int i = 0; i < candidatePool.Count; i++)
        {
            EmployeeData candidate = candidatePool[i];

            if (candidate == null)
                continue;

            if (originalRoster.Contains(candidate))
                continue;

            if (validCandidates.Contains(candidate))
                continue;

            validCandidates.Add(candidate);
        }

        Shuffle(validCandidates);

        int amount = Mathf.Min(recruitsPerWindow, validCandidates.Count);

        for (int i = 0; i < amount; i++)
            currentRecruitOptions.Add(validCandidates[i]);
    }

    private void GenerateRandomRecruitOptions()
    {
        HashSet<string> usedNamesThisWindow = new();

        int amount = Mathf.Max(1, recruitsPerWindow);

        for (int i = 0; i < amount; i++)
        {
            EmployeeData generatedRecruit = CreateGeneratedRecruit(usedNamesThisWindow);

            if (generatedRecruit != null)
                currentRecruitOptions.Add(generatedRecruit);
        }
    }

    private EmployeeData CreateGeneratedRecruit(HashSet<string> usedNamesThisWindow)
    {
        generatedRecruitSerial++;

        EmployeeData employee = ScriptableObject.CreateInstance<EmployeeData>();

        employee.name = $"Generated Recruit {generatedRecruitSerial}";
        employee.employeeName = GetRandomRecruitName(usedNamesThisWindow);
        employee.profilePicture = GetRandomRecruitPortrait();

        employee.cookingSkill = RollSkillForCurrentPrestige();
        employee.serviceSkill = RollSkillForCurrentPrestige();
        employee.operationalSkill = RollSkillForCurrentPrestige();
        employee.agility = RollSkillForCurrentPrestige();

        employee.maxStamina = RollMaxStaminaForCurrentPrestige();
        employee.currentStamina = employee.maxStamina;
        employee.availabilityState = EmployeeAvailabilityState.Available;

        employee.currentLevel = Mathf.Max(1, generatedStartingLevel);
        employee.currentXP = 0;
        employee.skillPoints = 0;

        employee.hasTrait = false;
        employee.traitName = "";

        employee.statusIconA = null;
        employee.statusIconB = null;

        employee.baseSalary = CalculateGeneratedBaseSalary(employee);

        return employee;
    }

    private int RollMaxStaminaForCurrentPrestige()
    {
        int minStamina = Mathf.Max(1, minGeneratedMaxStamina);
        int defaultStamina = Mathf.Max(minStamina, defaultGeneratedMaxStamina);
        int maxStamina = Mathf.Max(defaultStamina, maxGeneratedMaxStamina);

        int prestigeLevel = GetCurrentPrestigeLevelForRecruitment();

        float prestige01 = Mathf.InverseLerp(
            minPrestigeLevelForRecruitScaling,
            Mathf.Max(minPrestigeLevelForRecruitScaling, maxPrestigeLevelForRecruitScaling),
            prestigeLevel
        );

        float curvePower = Mathf.Lerp(lowPrestigeStaminaPower, highPrestigeStaminaPower, prestige01);

        float random01 = Mathf.Pow(UnityEngine.Random.value, curvePower);

        int rolledStamina = Mathf.RoundToInt(Mathf.Lerp(minStamina, maxStamina, random01));

        return Mathf.Clamp(rolledStamina, minStamina, maxStamina);
    }

    private int RollSkillForCurrentPrestige()
    {
        int minSkill = Mathf.Clamp(minGeneratedSkill, 0, 10);
        int maxSkill = Mathf.Clamp(maxGeneratedSkill, 0, 10);

        if (maxSkill < minSkill)
            (minSkill, maxSkill) = (maxSkill, minSkill);

        if (minSkill == maxSkill)
            return minSkill;

        int prestigeLevel = GetCurrentPrestigeLevelForRecruitment();

        float prestige01 = Mathf.InverseLerp(
            minPrestigeLevelForRecruitScaling,
            Mathf.Max(minPrestigeLevelForRecruitScaling, maxPrestigeLevelForRecruitScaling),
            prestigeLevel
        );

        float curvePower = Mathf.Lerp(lowPrestigeSkillPower, highPrestigeSkillPower, prestige01);

        float random01 = Mathf.Pow(UnityEngine.Random.value, curvePower);

        int rolledSkill = Mathf.RoundToInt(Mathf.Lerp(minSkill, maxSkill, random01));

        return Mathf.Clamp(rolledSkill, minSkill, maxSkill);
    }

    private int GetCurrentPrestigeLevelForRecruitment()
    {
        if (ReputationTierManager.Instance != null)
            return Mathf.Max(1, ReputationTierManager.Instance.CurrentTierLevel);

        if (RestaurantProgressionManager.Instance != null)
            return Mathf.Max(1, RestaurantProgressionManager.Instance.CurrentRestaurantLevel);

        return 1;
    }

    private string GetRandomRecruitName(HashSet<string> usedNamesThisWindow)
    {
        List<string> validNames = new();

        for (int i = 0; i < recruitNamePool.Count; i++)
        {
            string candidateName = recruitNamePool[i];

            if (string.IsNullOrWhiteSpace(candidateName))
                continue;

            if (usedNamesThisWindow != null && usedNamesThisWindow.Contains(candidateName))
                continue;

            validNames.Add(candidateName);
        }

        string selectedName;

        if (validNames.Count > 0)
        {
            selectedName = validNames[UnityEngine.Random.Range(0, validNames.Count)];
        }
        else if (recruitNamePool.Count > 0)
        {
            selectedName = recruitNamePool[UnityEngine.Random.Range(0, recruitNamePool.Count)];
        }
        else
        {
            selectedName = $"Recruit {generatedRecruitSerial}";
        }

        if (usedNamesThisWindow != null && !string.IsNullOrWhiteSpace(selectedName))
            usedNamesThisWindow.Add(selectedName);

        return selectedName;
    }

    private Sprite GetRandomRecruitPortrait()
    {
        if (allowEmptyPortrait && UnityEngine.Random.value < emptyPortraitChance)
            return null;

        if (recruitPortraitPool == null || recruitPortraitPool.Count == 0)
            return null;

        List<Sprite> validPortraits = new();

        for (int i = 0; i < recruitPortraitPool.Count; i++)
        {
            if (recruitPortraitPool[i] != null)
                validPortraits.Add(recruitPortraitPool[i]);
        }

        if (validPortraits.Count == 0)
            return null;

        return validPortraits[UnityEngine.Random.Range(0, validPortraits.Count)];
    }

    private int CalculateGeneratedBaseSalary(EmployeeData employee)
    {
        if (employee == null)
            return Mathf.Max(0, minGeneratedBaseSalary);

        int minSalary = Mathf.Max(0, minGeneratedBaseSalary);
        int maxSalary = Mathf.Max(minSalary, maxGeneratedBaseSalary);

        float averageSkill =
            (employee.cookingSkill +
             employee.serviceSkill +
             employee.operationalSkill +
             employee.agility) / 4f;

        float average01 = Mathf.InverseLerp(0f, 10f, averageSkill);

        int salary = Mathf.RoundToInt(Mathf.Lerp(minSalary, maxSalary, average01));

        return Mathf.Clamp(salary, minSalary, maxSalary);
    }

    private void BuildUI()
    {
        ClearUI();

        int rosterLimit = GetRosterLimit();
        int slotAmount = Mathf.Max(rosterLimit, workingRoster.Count);

        for (int i = 0; i < slotAmount; i++)
        {
            RecruitmentRosterSlotUI slot = Instantiate(rosterSlotPrefab, rosterSlotsContainer);
            slot.Setup(this);
            spawnedSlots.Add(slot);
        }

        for (int i = 0; i < originalRoster.Count; i++)
        {
            RecruitmentEmployeeCardUI card = CreateCard(originalRoster[i]);
            RecruitmentRosterSlotUI emptySlot = FindFirstEmptySlot();

            if (emptySlot != null)
                emptySlot.SetCard(card);
            else
                PlaceCardInPool(card);
        }

        for (int i = 0; i < currentRecruitOptions.Count; i++)
        {
            RecruitmentEmployeeCardUI card = CreateCard(currentRecruitOptions[i]);
            PlaceCardInPool(card);
        }
    }

    private RecruitmentEmployeeCardUI CreateCard(EmployeeData employee)
    {
        RecruitmentEmployeeCardUI card = Instantiate(employeeCardPrefab, recruitsContainer);
        card.Setup(employee, this);

        spawnedCards.Add(card);

        if (employee != null && !cardByEmployee.ContainsKey(employee))
            cardByEmployee.Add(employee, card);

        return card;
    }

    private void ClearUI()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
                Destroy(spawnedCards[i].gameObject);
        }

        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i].gameObject);
        }

        spawnedCards.Clear();
        spawnedSlots.Clear();
        cardByEmployee.Clear();
    }

    public void OnCardActionButtonClicked(RecruitmentEmployeeCardUI card)
    {
        if (card == null || card.Employee == null)
            return;

        if (workingRoster.Contains(card.Employee))
            TryRemoveFromRoster(card);
        else
            TryAddToRoster(card);
    }

    public bool TryDropCardOnRosterSlot(RecruitmentEmployeeCardUI card, RecruitmentRosterSlotUI targetSlot)
    {
        if (card == null || card.Employee == null || targetSlot == null)
            return false;

        EmployeeData employee = card.Employee;
        bool alreadyInRoster = workingRoster.Contains(employee);

        if (!alreadyInRoster && !HasRosterSpace())
            return false;

        RecruitmentRosterSlotUI sourceSlot = card.CurrentRosterSlot;

        if (targetSlot.CurrentCard == card)
            return true;

        if (targetSlot.CurrentCard != null && targetSlot.CurrentCard != card)
        {
            // Só permite troca entre dois cards que já estão no roster.
            // Não força demissão automática ao soltar candidato em slot ocupado.
            if (sourceSlot == null)
                return false;

            RecruitmentEmployeeCardUI oldCard = targetSlot.CurrentCard;

            targetSlot.ClearCard(oldCard);
            sourceSlot.ClearCard(card);

            targetSlot.SetCard(card);
            sourceSlot.SetCard(oldCard);

            RefreshAllVisuals();
            return true;
        }

        if (!alreadyInRoster)
            workingRoster.Add(employee);

        if (sourceSlot != null && sourceSlot != targetSlot)
            sourceSlot.ClearCard(card);

        targetSlot.SetCard(card);

        RefreshAllVisuals();
        return true;
    }

    public bool TryDropCardOnPool(RecruitmentEmployeeCardUI card)
    {
        if (card == null || card.Employee == null)
            return false;

        TryRemoveFromRoster(card);
        return true;
    }

    private bool TryAddToRoster(RecruitmentEmployeeCardUI card)
    {
        if (card == null || card.Employee == null)
            return false;

        if (workingRoster.Contains(card.Employee))
            return false;

        if (!HasRosterSpace())
            return false;

        RecruitmentRosterSlotUI emptySlot = FindFirstEmptySlot();

        if (emptySlot == null)
            return false;

        workingRoster.Add(card.Employee);
        emptySlot.SetCard(card);

        RefreshAllVisuals();
        return true;
    }

    private bool TryRemoveFromRoster(RecruitmentEmployeeCardUI card)
    {
        if (card == null || card.Employee == null)
            return false;

        if (workingRoster.Contains(card.Employee))
            workingRoster.Remove(card.Employee);

        PlaceCardInPool(card);

        RefreshAllVisuals();
        return true;
    }

    private void PlaceCardInPool(RecruitmentEmployeeCardUI card)
    {
        if (card == null)
            return;

        if (card.CurrentRosterSlot != null)
            card.CurrentRosterSlot.ClearCard(card);

        card.SetCurrentRosterSlot(null);
        card.MoveToParent(recruitsContainer);
    }

    private RecruitmentRosterSlotUI FindFirstEmptySlot()
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            RecruitmentRosterSlotUI slot = spawnedSlots[i];

            if (slot != null && slot.IsEmpty)
                return slot;
        }

        return null;
    }

    private bool HasRosterSpace()
    {
        return workingRoster.Count < GetRosterLimit();
    }

    private int GetRosterLimit()
    {
        if (EmployeeRosterManager.Instance != null)
            return EmployeeRosterManager.Instance.CurrentRosterLimit;

        return Mathf.Max(1, fallbackRosterLimit);
    }

    private void RefreshAllVisuals()
    {
        int rosterLimit = GetRosterLimit();

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            RecruitmentEmployeeCardUI card = spawnedCards[i];

            if (card == null || card.Employee == null)
                continue;

            bool isInRoster = workingRoster.Contains(card.Employee);
            bool wasOriginalStaff = originalRoster.Contains(card.Employee);

            RecruitmentCardState state = RecruitmentCardState.Neutral;

            if (isInRoster && !wasOriginalStaff)
                state = RecruitmentCardState.PendingHire;
            else if (!isInRoster && wasOriginalStaff)
                state = RecruitmentCardState.PendingDismiss;

            bool canUseAction = isInRoster || HasRosterSpace();

            card.SetState(state, isInRoster, canUseAction);
        }

        if (currentStaffCountText != null)
            currentStaffCountText.text = $"({workingRoster.Count}/{rosterLimit})";

        if (availableRecruitsCountText != null)
            availableRecruitsCountText.text = $"({CountCardsOutsideRoster()})";

        int workingPayroll = CalculateWorkingPayroll();

        if (payrollIndicatorUI != null)
        {
            payrollIndicatorUI.Refresh(workingPayroll, originalPayrollAtWindowOpen);
        }
        else if (payrollText != null)
        {
            payrollText.text = $"${workingPayroll}/DAY";
        }

        bool canConfirm = workingRoster.Count >= minimumRosterSize;

        if (chargeHiringCostsOnConfirm && ResourceManager.Instance != null)
            canConfirm = canConfirm && ResourceManager.Instance.CurrentMoney >= CalculatePendingHireCost();

        if (confirmButton != null)
            confirmButton.interactable = canConfirm;

        ForceRebuildLayouts();
    }

    private int CountCardsOutsideRoster()
    {
        int count = 0;

        for (int i = 0; i < spawnedCards.Count; i++)
        {
            RecruitmentEmployeeCardUI card = spawnedCards[i];

            if (card == null || card.Employee == null)
                continue;

            if (!workingRoster.Contains(card.Employee))
                count++;
        }

        return count;
    }

    private int CalculateWorkingPayroll()
    {
        return CalculatePayroll(workingRoster);
    }

    private int CalculatePayroll(IReadOnlyList<EmployeeData> employees)
    {
        if (employees == null)
            return 0;

        int total = 0;

        for (int i = 0; i < employees.Count; i++)
        {
            EmployeeData employee = employees[i];

            if (employee == null)
                continue;

            total += employee.GetDailyCost();
        }

        return total;
    }

    private int CalculatePendingHireCost()
    {
        int total = 0;

        for (int i = 0; i < workingRoster.Count; i++)
        {
            EmployeeData employee = workingRoster[i];

            if (employee == null)
                continue;

            if (!originalRoster.Contains(employee))
                total += employee.hireCost;
        }

        return total;
    }

    private void ConfirmChanges()
    {
        if (workingRoster.Count < minimumRosterSize)
            return;

        int hireCost = CalculatePendingHireCost();

        if (chargeHiringCostsOnConfirm && hireCost > 0 && ResourceManager.Instance != null)
        {
            bool spent = ResourceManager.Instance.TrySpendMoney(hireCost);

            if (!spent)
                return;
        }

        if (EmployeeRosterManager.Instance != null)
            EmployeeRosterManager.Instance.SetEmployees(new List<EmployeeData>(workingRoster));

        if (EmployeeRuntimeManager.Instance != null)
            EmployeeRuntimeManager.Instance.RegisterEmployees(workingRoster);

        CloseAndContinue();
    }

    private void CancelChanges()
    {
        CloseAndContinue();
    }

    private void CloseAndContinue()
    {
        IsRecruitmentOpen = false;
        if (screenRoot != null)
            screenRoot.SetActive(false);

        ClearUI();

        RestoreSidebarEmployee();

        Action callback = onRecruitmentFinished;
        onRecruitmentFinished = null;

        callback?.Invoke();
    }

    private void ForceRebuildLayouts()
    {
        Canvas.ForceUpdateCanvases();

        if (rosterSlotsContainer is RectTransform rosterRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rosterRect);

        if (recruitsContainer is RectTransform recruitsRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(recruitsRect);

        Canvas.ForceUpdateCanvases();
    }

    private void Shuffle(List<EmployeeData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void OnValidate()
    {
        minGeneratedMaxStamina = Mathf.Max(1, minGeneratedMaxStamina);
        defaultGeneratedMaxStamina = Mathf.Max(minGeneratedMaxStamina, defaultGeneratedMaxStamina);
        maxGeneratedMaxStamina = Mathf.Max(defaultGeneratedMaxStamina, maxGeneratedMaxStamina);

        lowPrestigeStaminaPower = Mathf.Max(0.1f, lowPrestigeStaminaPower);
        highPrestigeStaminaPower = Mathf.Max(0.1f, highPrestigeStaminaPower);
    }
}