using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum RecruitmentCardState
{
    Neutral,
    PendingHire,
    PendingDismiss
}

public class RecruitmentEmployeeCardUI : MonoBehaviour
{
    [Header("Base Card")]
    [SerializeField] private EmployeeCardUI employeeCardUI;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI salaryText;

    [Header("Action Button")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Image actionButtonImage;

    [Header("Action Button Colors")]
    [SerializeField] private Color hireButtonColor = new Color(0.25f, 0.65f, 0.35f, 1f);
    [SerializeField] private Color dismissButtonColor = new Color(0.75f, 0.2f, 0.16f, 1f);
    [SerializeField] private Color disabledButtonColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);

    [Header("Selection Border")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Color neutralBorderColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private Color pendingHireBorderColor = Color.green;
    [SerializeField] private Color pendingDismissBorderColor = Color.red;

    private StaffRecruitmentManager owner;

    public EmployeeData Employee { get; private set; }
    public RecruitmentRosterSlotUI CurrentRosterSlot { get; private set; }

    private void Awake()
    {
        if (employeeCardUI == null)
            employeeCardUI = GetComponent<EmployeeCardUI>();

        if (employeeCardUI == null)
            employeeCardUI = GetComponentInChildren<EmployeeCardUI>(true);

        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnActionButtonClicked);
            actionButton.onClick.AddListener(OnActionButtonClicked);

            if (actionButtonImage == null)
                actionButtonImage = actionButton.GetComponent<Image>();
        }
    }

    public void Setup(EmployeeData employee, StaffRecruitmentManager ownerManager)
    {
        Employee = employee;
        owner = ownerManager;

        if (employeeCardUI != null)
            employeeCardUI.Setup(employee);

        RefreshSalaryText();
    }

    private void RefreshSalaryText()
    {
        if (salaryText == null)
            return;

        if (Employee == null)
        {
            salaryText.text = "";
            return;
        }

        salaryText.text = $"${Employee.GetDailyCost()}/DAY";
    }

    public void SetState(RecruitmentCardState state, bool isInRoster, bool canUseAction)
    {
        ApplyBorderState(state);
        ApplyActionButtonState(isInRoster, canUseAction);
    }

    private void ApplyBorderState(RecruitmentCardState state)
    {
        if (borderImage == null)
            return;

        switch (state)
        {
            case RecruitmentCardState.PendingHire:
                borderImage.color = pendingHireBorderColor;
                borderImage.gameObject.SetActive(true);
                break;

            case RecruitmentCardState.PendingDismiss:
                borderImage.color = pendingDismissBorderColor;
                borderImage.gameObject.SetActive(true);
                break;

            default:
                borderImage.color = neutralBorderColor;
                borderImage.gameObject.SetActive(false);
                break;
        }
    }

    private void ApplyActionButtonState(bool isInRoster, bool canUseAction)
    {
        bool shouldDismiss = isInRoster;

        if (actionButtonText != null)
            actionButtonText.text = shouldDismiss ? "DISMISS" : "HIRE";

        Color targetColor = shouldDismiss ? dismissButtonColor : hireButtonColor;

        if (!canUseAction)
            targetColor = disabledButtonColor;

        ApplyButtonColor(targetColor);

        if (actionButton != null)
            actionButton.interactable = canUseAction;
    }

    private void ApplyButtonColor(Color targetColor)
    {
        if (actionButtonImage != null)
            actionButtonImage.color = targetColor;

        if (actionButton == null)
            return;

        ColorBlock colors = actionButton.colors;

        colors.normalColor = targetColor;
        colors.highlightedColor = Color.Lerp(targetColor, Color.white, 0.15f);
        colors.pressedColor = Color.Lerp(targetColor, Color.black, 0.15f);
        colors.selectedColor = targetColor;
        colors.disabledColor = disabledButtonColor;

        actionButton.colors = colors;
    }

    public void SetCurrentRosterSlot(RecruitmentRosterSlotUI slot)
    {
        CurrentRosterSlot = slot;
    }

    public void MoveToParent(Transform targetParent, int siblingIndex = -1)
    {
        if (targetParent == null)
            return;

        transform.SetParent(targetParent, false);

        if (siblingIndex >= 0 && siblingIndex < targetParent.childCount)
            transform.SetSiblingIndex(siblingIndex);

        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.localPosition = Vector3.zero;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.localScale = Vector3.one;
    }

    private void OnActionButtonClicked()
    {
        owner?.OnCardActionButtonClicked(this);
    }
}