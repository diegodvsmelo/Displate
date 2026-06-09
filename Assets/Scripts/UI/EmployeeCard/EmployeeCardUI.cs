using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EmployeeCardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Main Info")]
    [SerializeField] private Image profileImage;
    [SerializeField] private Image levelUpIcon;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI traitText;

    [Header("Status Slots")]
    [SerializeField] private Image statusIconSlotA;
    [SerializeField] private Image statusIconSlotB;

    [Header("Bars")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TextMeshProUGUI staminaValueText;
    [SerializeField] private string staminaValueFormat = "{0}/{1}";

    [SerializeField] private Slider xpSlider;

    [Header("Availability Overlay")]
    [SerializeField] private CanvasGroup availabilityOverlayGroup;
    [SerializeField] private TextMeshProUGUI availabilityText;
    [SerializeField] private float overlayMaxAlpha = 0.5f;

    [Header("Manual Rest (Compact Card Only)")]
    [SerializeField] private bool enableManualRestButton = false;
    [SerializeField] private RectTransform manualRestButtonRoot;
    [SerializeField] private CanvasGroup manualRestButtonCanvasGroup;
    [SerializeField] private Button manualRestButton;
    [SerializeField] private float manualRestButtonExpandDuration = 0.12f;
    [SerializeField] private Vector2 manualRestButtonBaseSize = new Vector2(120f, 36f);
    [SerializeField] private float manualRestButtonOffset = 8f;
    [SerializeField] private float disabledButtonAlpha = 0.55f;

    [Header("Expanded Stats - Optional")]
    [SerializeField] private AttributeSquaresUI cookingSquares;
    [SerializeField] private AttributeSquaresUI serviceSquares;
    [SerializeField] private AttributeSquaresUI operationalSquares;
    [SerializeField] private AttributeSquaresUI agilitySquares;

    private EmployeeData employeeData;
    private RectTransform cardRectTransform;
    private EmployeeCardDraggable cardDraggable;
    private Coroutine manualRestButtonRoutine;
    private int lastManualRestButtonOpenedFrame = -1;

    public EmployeeData Data => employeeData;

    private void Awake()
    {
        cardRectTransform = GetComponent<RectTransform>();
        cardDraggable = GetComponent<EmployeeCardDraggable>();

        if (manualRestButton != null)
        {
            manualRestButton.onClick.RemoveListener(OnManualRestButtonClicked);
            manualRestButton.onClick.AddListener(OnManualRestButtonClicked);
        }

        HideManualRestButtonInstant();
        HideOverlayInstant();
    }

    private void Update()
    {
        HandleOutsideClickToHideRestButton();
    }

    public void Setup(EmployeeData data)
    {
        UnsubscribeFromCurrentData();

        employeeData = data;

        SubscribeToCurrentData();
        Refresh();
    }

    private void OnDestroy()
    {
        UnsubscribeFromCurrentData();
    }

    private void OnDisable()
    {
        HideManualRestButtonInstant();
    }

    private void SubscribeToCurrentData()
    {
        if (employeeData != null)
            employeeData.OnDataChanged += Refresh;
    }

    private void UnsubscribeFromCurrentData()
    {
        if (employeeData != null)
            employeeData.OnDataChanged -= Refresh;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enableManualRestButton)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (cardDraggable != null && (cardDraggable.IsDragging || cardDraggable.WasRecentlyDragged()))
            return;

        if (!CanShowManualRestButton())
        {
            HideManualRestButtonInstant();
            return;
        }

        if (manualRestButtonRoot != null && manualRestButtonRoot.gameObject.activeSelf)
        {
            HideManualRestButtonInstant();
            return;
        }

        ShowManualRestButtonAt(eventData);
    }

    public void Refresh()
    {
        if (employeeData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (profileImage != null)
            profileImage.sprite = employeeData.profilePicture;

        if (nameText != null)
            nameText.text = employeeData.employeeName;

        if (levelText != null)
            levelText.text = $"Lvl. {employeeData.currentLevel}";

        if (traitText != null)
        {
            bool hasTrait = employeeData.HasTrait();
            traitText.gameObject.SetActive(hasTrait);
            traitText.text = hasTrait ? employeeData.traitName : "";
        }

        if (levelUpIcon != null)
            levelUpIcon.gameObject.SetActive(employeeData.HasUnspentSkillPoints());

        UpdateStatusIcon(statusIconSlotA, employeeData.statusIconA);
        UpdateStatusIcon(statusIconSlotB, employeeData.statusIconB);

        if (staminaSlider != null)
            UpdateStaminaUI();

        if (xpSlider != null)
            xpSlider.value = employeeData.GetXpPercent();

        UpdateAttributeSquares();
        UpdateAvailabilityOverlay();
        UpdateManualRestButtonVisibility();
    }

    private void UpdateStaminaUI()
    {
        if (employeeData == null)
            return;

        if (staminaSlider != null)
            staminaSlider.value = employeeData.GetStaminaPercent();

        if (staminaValueText != null)
        {
            int currentStamina = Mathf.Clamp(employeeData.currentStamina, 0, employeeData.maxStamina);
            int maxStamina = Mathf.Max(1, employeeData.maxStamina);

            staminaValueText.text = string.Format(staminaValueFormat, currentStamina, maxStamina);
        }
    }
    private void UpdateAttributeSquares()
    {
        if (employeeData == null)
            return;

        if (cookingSquares != null)
            cookingSquares.UpdateValue(employeeData.cookingSkill);

        if (serviceSquares != null)
            serviceSquares.UpdateValue(employeeData.serviceSkill);

        if (operationalSquares != null)
            operationalSquares.UpdateValue(employeeData.operationalSkill);

        if (agilitySquares != null)
            agilitySquares.UpdateValue(employeeData.agility);
    }

    private void UpdateStatusIcon(Image slot, Sprite icon)
    {
        if (slot == null)
            return;

        bool hasIcon = icon != null;

        slot.gameObject.SetActive(hasIcon);
        slot.sprite = icon;
    }

    private void UpdateAvailabilityOverlay()
    {
        if (availabilityOverlayGroup == null)
            return;

        string overlayLabel = "";
        bool shouldShowOverlay = false;

        if (employeeData != null)
        {
            if (employeeData.IsOccupied())
            {
                overlayLabel = "OCCUPIED";
                shouldShowOverlay = true;
            }
            else if (employeeData.IsResting())
            {
                overlayLabel = "RESTING";
                shouldShowOverlay = true;
            }
        }

        if (!shouldShowOverlay)
        {
            HideOverlayInstant();
            return;
        }

        if (availabilityText != null)
        {
            availabilityText.text = overlayLabel;
            availabilityText.gameObject.SetActive(true);
        }

        if (!availabilityOverlayGroup.gameObject.activeSelf)
            availabilityOverlayGroup.gameObject.SetActive(true);

        availabilityOverlayGroup.alpha = overlayMaxAlpha;
        availabilityOverlayGroup.blocksRaycasts = true;
        availabilityOverlayGroup.interactable = false;
    }

    private void HideOverlayInstant()
    {
        if (availabilityOverlayGroup == null)
            return;

        availabilityOverlayGroup.alpha = 0f;
        availabilityOverlayGroup.blocksRaycasts = false;
        availabilityOverlayGroup.interactable = false;

        if (availabilityText != null)
        {
            availabilityText.text = "";
            availabilityText.gameObject.SetActive(false);
        }

        availabilityOverlayGroup.gameObject.SetActive(false);
    }

    private void UpdateManualRestButtonVisibility()
    {
        if (!CanShowManualRestButton())
        {
            HideManualRestButtonInstant();
            return;
        }

        if (manualRestButtonRoot != null && manualRestButtonRoot.gameObject.activeSelf)
            UpdateManualRestButtonInteractivity();
    }

    private bool CanShowManualRestButton()
    {
        if (!enableManualRestButton)
            return false;

        if (employeeData == null)
            return false;

        if (!employeeData.IsAvailable())
            return false;

        if (manualRestButtonRoot == null)
            return false;

        if (manualRestButton == null)
            return false;

        return true;
    }

    private bool CanUseManualRestButton()
    {
        if (!CanShowManualRestButton())
            return false;

        return employeeData.currentStamina < employeeData.maxStamina;
    }

    private void ShowManualRestButtonAt(PointerEventData eventData)
    {
        if (manualRestButtonRoot == null)
            return;

        if (cardRectTransform == null)
            return;

        if (manualRestButtonRoutine != null)
        {
            StopCoroutine(manualRestButtonRoutine);
            manualRestButtonRoutine = null;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardRectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        manualRestButtonRoot.anchorMin = new Vector2(0.5f, 0.5f);
        manualRestButtonRoot.anchorMax = new Vector2(0.5f, 0.5f);
        manualRestButtonRoot.pivot = new Vector2(1f, 0.5f);
        manualRestButtonRoot.anchoredPosition = localPoint + new Vector2(-manualRestButtonOffset, 0f);

        if (!manualRestButtonRoot.gameObject.activeSelf)
            manualRestButtonRoot.gameObject.SetActive(true);

        UpdateManualRestButtonInteractivity();

        manualRestButtonRoot.sizeDelta = new Vector2(0f, manualRestButtonBaseSize.y);
        lastManualRestButtonOpenedFrame = Time.frameCount;

        if (!gameObject.activeInHierarchy)
        {
            manualRestButtonRoot.sizeDelta = manualRestButtonBaseSize;
            return;
        }

        manualRestButtonRoutine = StartCoroutine(AnimateManualRestButtonRoutine());
    }

    private void UpdateManualRestButtonInteractivity()
    {
        bool canUse = CanUseManualRestButton();

        if (manualRestButton != null)
            manualRestButton.interactable = canUse;

        if (manualRestButtonCanvasGroup != null)
        {
            manualRestButtonCanvasGroup.alpha = canUse ? 1f : disabledButtonAlpha;
            manualRestButtonCanvasGroup.blocksRaycasts = true;
            manualRestButtonCanvasGroup.interactable = canUse;
        }
    }

    private IEnumerator AnimateManualRestButtonRoutine()
    {
        float elapsed = 0f;
        Vector2 startSize = new Vector2(0f, manualRestButtonBaseSize.y);

        while (elapsed < manualRestButtonExpandDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / manualRestButtonExpandDuration);

            manualRestButtonRoot.sizeDelta = Vector2.Lerp(startSize, manualRestButtonBaseSize, t);
            yield return null;
        }

        manualRestButtonRoot.sizeDelta = manualRestButtonBaseSize;
        manualRestButtonRoutine = null;
    }

    private void HideManualRestButtonInstant()
    {
        if (manualRestButtonRoutine != null)
        {
            StopCoroutine(manualRestButtonRoutine);
            manualRestButtonRoutine = null;
        }

        if (manualRestButtonCanvasGroup != null)
        {
            manualRestButtonCanvasGroup.alpha = 0f;
            manualRestButtonCanvasGroup.blocksRaycasts = false;
            manualRestButtonCanvasGroup.interactable = false;
        }

        if (manualRestButtonRoot != null)
        {
            manualRestButtonRoot.sizeDelta = new Vector2(0f, manualRestButtonBaseSize.y);
            manualRestButtonRoot.gameObject.SetActive(false);
        }
    }

    private void HandleOutsideClickToHideRestButton()
    {
        if (!enableManualRestButton)
            return;

        if (manualRestButtonRoot == null || !manualRestButtonRoot.gameObject.activeSelf)
            return;

        if (!gameObject.activeInHierarchy)
        {
            HideManualRestButtonInstant();
            return;
        }

        if (Time.frameCount == lastManualRestButtonOpenedFrame)
            return;

        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;

        bool anyClickThisFrame =
            mouse.leftButton.wasPressedThisFrame ||
            mouse.rightButton.wasPressedThisFrame;

        if (!anyClickThisFrame)
            return;

        if (IsPointerOverManualRestButton())
            return;

        HideManualRestButtonInstant();
    }

    private bool IsPointerOverManualRestButton()
    {
        if (manualRestButtonRoot == null || EventSystem.current == null || Mouse.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        for (int i = 0; i < results.Count; i++)
        {
            Transform hitTransform = results[i].gameObject.transform;

            if (hitTransform == manualRestButtonRoot || hitTransform.IsChildOf(manualRestButtonRoot))
                return true;
        }

        return false;
    }

    private void OnManualRestButtonClicked()
    {
        if (!CanUseManualRestButton())
            return;

        employeeData.SetResting();
        HideManualRestButtonInstant();
    }
}