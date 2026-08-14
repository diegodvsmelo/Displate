using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSheetUI : MonoBehaviour
{
    public static CharacterSheetUI Instance { get; private set; }

    [Header("Screen")]
    [SerializeField] private GameObject screenRoot;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    [Header("Employee Information")]
    [SerializeField] private Image profileImage;
    [SerializeField] private TextMeshProUGUI employeeNameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI salaryText;

    [Header("Experience")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TextMeshProUGUI xpValueText;

    [Header("Stamina")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaValueText;

    [Header("Trait")]
    [SerializeField] private GameObject traitSectionRoot;
    [SerializeField] private TextMeshProUGUI traitText;

    [Header("Skill Values")]
    [SerializeField] private AttributeSquaresUI cookingSquares;
    [SerializeField] private AttributeSquaresUI serviceSquares;
    [SerializeField] private AttributeSquaresUI operationalSquares;
    [SerializeField] private AttributeSquaresUI agilitySquares;

    [Header("Skill Buttons")]
    [SerializeField] private Button cookingMinusButton;
    [SerializeField] private Button cookingPlusButton;

    [SerializeField] private Button serviceMinusButton;
    [SerializeField] private Button servicePlusButton;

    [SerializeField] private Button operationalMinusButton;
    [SerializeField] private Button operationalPlusButton;

    [SerializeField] private Button agilityMinusButton;
    [SerializeField] private Button agilityPlusButton;

    [SerializeField] private TextMeshProUGUI cookingValueText;
    [SerializeField] private TextMeshProUGUI serviceValueText;
    [SerializeField] private TextMeshProUGUI operationalValueText;
    [SerializeField] private TextMeshProUGUI agilityValueText;

    [Header("Available Points")]
    [SerializeField] private TextMeshProUGUI availablePointsText;

    private int temporaryPoints;
    private int temporaryCooking;
    private int temporaryService;
    private int temporaryOperational;
    private int temporaryAgility;
    public event Action<EmployeeData> OnSheetConfirmed;
    public event Action<EmployeeData> OnSheetOpened;
    public event Action<EmployeeData> OnSheetClosedWithoutSaving;

    private EmployeeData currentData;
    private Action onUpdateCallback;
    private bool shouldResumeGameWhenClosed;

    public EmployeeData CurrentData => currentData;

    public bool IsOpen =>
        screenRoot != null &&
        screenRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RegisterButtonListeners();
        
        if (screenRoot != null)
            screenRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        UnregisterButtonListeners();

        if (Instance == this)
            Instance = null;
    }

    private void RegisterButtonListeners()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseWithoutSaving);
            closeButton.onClick.AddListener(CloseWithoutSaving);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CloseWithoutSaving);
            cancelButton.onClick.AddListener(CloseWithoutSaving);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmChanges);
            confirmButton.onClick.AddListener(ConfirmChanges);
        }

        RegisterSkillButtonListeners();
    }

    private void UnregisterButtonListeners()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(CloseWithoutSaving);

        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CloseWithoutSaving);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(ConfirmChanges);

        UnregisterSkillButtonListeners();
    }

    private void RegisterSkillButtonListeners()
    {
        if (cookingMinusButton != null)
        {
            cookingMinusButton.onClick.RemoveListener(OnCookingMinusClicked);
            cookingMinusButton.onClick.AddListener(OnCookingMinusClicked);
        }

        if (cookingPlusButton != null)
        {
            cookingPlusButton.onClick.RemoveListener(OnCookingPlusClicked);
            cookingPlusButton.onClick.AddListener(OnCookingPlusClicked);
        }

        if (serviceMinusButton != null)
        {
            serviceMinusButton.onClick.RemoveListener(OnServiceMinusClicked);
            serviceMinusButton.onClick.AddListener(OnServiceMinusClicked);
        }

        if (servicePlusButton != null)
        {
            servicePlusButton.onClick.RemoveListener(OnServicePlusClicked);
            servicePlusButton.onClick.AddListener(OnServicePlusClicked);
        }

        if (operationalMinusButton != null)
        {
            operationalMinusButton.onClick.RemoveListener(OnOperationalMinusClicked);
            operationalMinusButton.onClick.AddListener(OnOperationalMinusClicked);
        }

        if (operationalPlusButton != null)
        {
            operationalPlusButton.onClick.RemoveListener(OnOperationalPlusClicked);
            operationalPlusButton.onClick.AddListener(OnOperationalPlusClicked);
        }

        if (agilityMinusButton != null)
        {
            agilityMinusButton.onClick.RemoveListener(OnAgilityMinusClicked);
            agilityMinusButton.onClick.AddListener(OnAgilityMinusClicked);
        }

        if (agilityPlusButton != null)
        {
            agilityPlusButton.onClick.RemoveListener(OnAgilityPlusClicked);
            agilityPlusButton.onClick.AddListener(OnAgilityPlusClicked);
        }
    }

    private void UnregisterSkillButtonListeners()
    {
        if (cookingMinusButton != null)
            cookingMinusButton.onClick.RemoveListener(OnCookingMinusClicked);

        if (cookingPlusButton != null)
            cookingPlusButton.onClick.RemoveListener(OnCookingPlusClicked);

        if (serviceMinusButton != null)
            serviceMinusButton.onClick.RemoveListener(OnServiceMinusClicked);

        if (servicePlusButton != null)
            servicePlusButton.onClick.RemoveListener(OnServicePlusClicked);

        if (operationalMinusButton != null)
            operationalMinusButton.onClick.RemoveListener(OnOperationalMinusClicked);

        if (operationalPlusButton != null)
            operationalPlusButton.onClick.RemoveListener(OnOperationalPlusClicked);

        if (agilityMinusButton != null)
            agilityMinusButton.onClick.RemoveListener(OnAgilityMinusClicked);

        if (agilityPlusButton != null)
            agilityPlusButton.onClick.RemoveListener(OnAgilityPlusClicked);
    }

    public void OpenSheet(EmployeeData data, Action onUpdate = null)
    {
        if (data == null)
        {
            Debug.LogWarning("[CharacterSheetUI] EmployeeData não informado.");
            return;
        }

        if (screenRoot == null)
        {
            Debug.LogWarning("[CharacterSheetUI] Screen Root não foi configurado.");
            return;
        }

        if (IsOpen)
            return;

        currentData = data;
        onUpdateCallback = onUpdate;

        temporaryPoints = Mathf.Max(0, data.skillPoints);
        temporaryCooking = Mathf.Clamp(data.cookingSkill, 0, 10);
        temporaryService = Mathf.Clamp(data.serviceSkill, 0, 10);
        temporaryOperational = Mathf.Clamp(data.operationalSkill, 0, 10);
        temporaryAgility = Mathf.Clamp(data.agility, 0, 10);

        GameManager gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            shouldResumeGameWhenClosed = !gameManager.isGamePaused;

            if (shouldResumeGameWhenClosed)
                gameManager.PauseGame();
        }
        else
        {
            shouldResumeGameWhenClosed = false;
            Debug.LogWarning("[CharacterSheetUI] GameManager não encontrado. O jogo não foi pausado.");
        }

        screenRoot.SetActive(true);
        RefreshEmployeeInformation();

        OnSheetOpened?.Invoke(currentData);
    }

    private void RefreshEmployeeInformation()
    {
        if (currentData == null)
            return;

        RefreshIdentity();
        RefreshProgressBars();
        RefreshTrait();
        RefreshSkills();

        RefreshAvailablePoints();
        RefreshSkillButtonInteractivity();
        RefreshConfirmButtonInteractivity();
    }

    private void RefreshIdentity()
    {
        if (profileImage != null)
        {
            profileImage.sprite = currentData.profilePicture;
            profileImage.enabled = currentData.profilePicture != null;
            profileImage.preserveAspect = true;
        }

        if (employeeNameText != null)
            employeeNameText.text = currentData.employeeName.ToUpperInvariant();

        if (levelText != null)
            levelText.text = $"LVL. {currentData.currentLevel}";

        if (salaryText != null)
            salaryText.text = $"${currentData.GetDailyCost()}/DAY";
    }

    private void RefreshProgressBars()
    {
        int xpRequired = Mathf.Max(1, currentData.GetXpToNextLevel());
        int currentXP = Mathf.Clamp(currentData.currentXP, 0, xpRequired);

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.value = currentData.GetXpPercent();
            xpSlider.interactable = false;
        }

        if (xpValueText != null)
            xpValueText.text = $"XP {currentXP} / {xpRequired}";

        int maximumStamina = Mathf.Max(1, currentData.maxStamina);
        int currentStamina =
            Mathf.Clamp(currentData.currentStamina, 0, maximumStamina);

        if (staminaSlider != null)
        {
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
            staminaSlider.value = currentData.GetStaminaPercent();
            staminaSlider.interactable = false;
        }

        if (staminaValueText != null)
        {
            staminaValueText.text =
                $"STAMINA {currentStamina} / {maximumStamina}";
        }
    }

    private void RefreshTrait()
    {
        bool hasTrait = currentData.HasTrait();

        if (traitSectionRoot != null)
            traitSectionRoot.SetActive(hasTrait);

        if (traitText != null)
            traitText.text = hasTrait
                ? currentData.traitName.ToUpperInvariant()
                : "";
    }

    private void RefreshSkills()
    {
        UpdateSkillDisplay(
            cookingSquares,
            cookingValueText,
            temporaryCooking
        );

        UpdateSkillDisplay(
            serviceSquares,
            serviceValueText,
            temporaryService
        );

        UpdateSkillDisplay(
            operationalSquares,
            operationalValueText,
            temporaryOperational
        );

        UpdateSkillDisplay(
            agilitySquares,
            agilityValueText,
            temporaryAgility
        );
    }

    private void UpdateSkillDisplay(
        AttributeSquaresUI squares,
        TextMeshProUGUI valueText,
        int value
    )
    {
        int safeValue = Mathf.Clamp(value, 0, 10);

        if (squares != null)
            squares.UpdateValue(safeValue);

        if (valueText != null)
            valueText.text = safeValue.ToString();
    }

    private void OnCookingMinusClicked()
    {
        TryModifySkill(
            ref temporaryCooking,
            currentData.cookingSkill,
            -1
        );
    }

    private void OnCookingPlusClicked()
    {
        TryModifySkill(
            ref temporaryCooking,
            currentData.cookingSkill,
            1
        );
    }

    private void OnServiceMinusClicked()
    {
        TryModifySkill(
            ref temporaryService,
            currentData.serviceSkill,
            -1
        );
    }

    private void OnServicePlusClicked()
    {
        TryModifySkill(
            ref temporaryService,
            currentData.serviceSkill,
            1
        );
    }

    private void OnOperationalMinusClicked()
    {
        TryModifySkill(
            ref temporaryOperational,
            currentData.operationalSkill,
            -1
        );
    }

    private void OnOperationalPlusClicked()
    {
        TryModifySkill(
            ref temporaryOperational,
            currentData.operationalSkill,
            1
        );
    }

    private void OnAgilityMinusClicked()
    {
        TryModifySkill(
            ref temporaryAgility,
            currentData.agility,
            -1
        );
    }

    private void OnAgilityPlusClicked()
    {
        TryModifySkill(
            ref temporaryAgility,
            currentData.agility,
            1
        );
    }

    private void TryModifySkill(
        ref int temporaryValue,
        int originalValue,
        int change
    )
    {
        if (currentData == null || currentData.IsOccupied())
            return;

        if (change > 0)
        {
            if (temporaryPoints <= 0)
                return;

            if (temporaryValue >= 10)
                return;

            temporaryValue++;
            temporaryPoints--;
        }
        else if (change < 0)
        {
            // Impede diminuir pontos que o funcionário já possuía
            // antes de abrir a janela.
            if (temporaryValue <= originalValue)
                return;

            temporaryValue--;
            temporaryPoints++;
        }

        RefreshSkills();
        RefreshAvailablePoints();
        RefreshSkillButtonInteractivity();
        RefreshConfirmButtonInteractivity();
    }

    private void RefreshAvailablePoints()
    {
        if (availablePointsText != null)
            availablePointsText.text = temporaryPoints.ToString();
    }

    private void RefreshSkillButtonInteractivity()
    {
        if (currentData == null)
            return;

        bool canSpendPoint = temporaryPoints > 0;

        SetSkillButtonsInteractivity(
            cookingMinusButton,
            cookingPlusButton,
            temporaryCooking,
            currentData.cookingSkill,
            canSpendPoint
        );

        SetSkillButtonsInteractivity(
            serviceMinusButton,
            servicePlusButton,
            temporaryService,
            currentData.serviceSkill,
            canSpendPoint
        );

        SetSkillButtonsInteractivity(
            operationalMinusButton,
            operationalPlusButton,
            temporaryOperational,
            currentData.operationalSkill,
            canSpendPoint
        );

        SetSkillButtonsInteractivity(
            agilityMinusButton,
            agilityPlusButton,
            temporaryAgility,
            currentData.agility,
            canSpendPoint
        );
    }

    private void SetSkillButtonsInteractivity(
        Button minusButton,
        Button plusButton,
        int temporaryValue,
        int originalValue,
        bool canSpendPoint
    )
    {
        if (minusButton != null)
            minusButton.interactable = temporaryValue > originalValue;

        if (plusButton != null)
        {
            plusButton.interactable =
                canSpendPoint &&
                temporaryValue < 10;
        }
    }

    private bool HasPendingChanges()
    {
        if (currentData == null)
            return false;

        return temporaryCooking != currentData.cookingSkill ||
               temporaryService != currentData.serviceSkill ||
               temporaryOperational != currentData.operationalSkill ||
               temporaryAgility != currentData.agility;
    }

    private void RefreshConfirmButtonInteractivity()
    {
        if (confirmButton != null)
            confirmButton.interactable = HasPendingChanges();
    }

    public void ConfirmChanges()
    {
        if (!IsOpen || currentData == null)
            return;

        if (!HasPendingChanges())
            return;

        EmployeeData confirmedEmployee = currentData;

        confirmedEmployee.cookingSkill = temporaryCooking;
        confirmedEmployee.serviceSkill = temporaryService;
        confirmedEmployee.operationalSkill = temporaryOperational;
        confirmedEmployee.agility = temporaryAgility;
        confirmedEmployee.skillPoints = temporaryPoints;

        confirmedEmployee.NotifyDataChanged();
        onUpdateCallback?.Invoke();

        OnSheetConfirmed?.Invoke(confirmedEmployee);

        CloseScreen();
    }

    private void CloseScreen()
    {
        if (screenRoot != null)
            screenRoot.SetActive(false);

        if (shouldResumeGameWhenClosed && GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        shouldResumeGameWhenClosed = false;
        currentData = null;
        onUpdateCallback = null;
    }

    public void CloseWithoutSaving()
    {
        if (!IsOpen)
            return;

        EmployeeData closedEmployee = currentData;

        CloseScreen();

        OnSheetClosedWithoutSaving?.Invoke(closedEmployee);
    }
}