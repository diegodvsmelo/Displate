using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DayCycleManager : MonoBehaviour
{
    public static DayCycleManager Instance { get; private set; }

    [Header("Daily Report Buttons")]
    [SerializeField] private Button startNextDayButton;

    [Header("Top Bar UI")]
    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private RectTransform clockRoot;

    [Header("Report Text Colors")]
    [SerializeField] private Color positiveValueColor = Color.green;
    [SerializeField] private Color negativeValueColor = Color.red;
    [SerializeField] private Color neutralValueColor = Color.white;

    [Header("Daily Report Panel")]
    [SerializeField] private GameObject dailyReportPanel;

    [Header("Tasks Processed")]
    [SerializeField] private TextMeshProUGUI tasksProcessedDetailsText;
    [SerializeField] private TextMeshProUGUI successRateText;

    [Header("Money Summary")]
    [SerializeField] private TextMeshProUGUI moneySummaryDetailsText;
    [SerializeField] private TextMeshProUGUI netProfitText;

    [Header("Reputation Summary")]
    [SerializeField] private TextMeshProUGUI reputationSummaryDetailsText;
    [SerializeField] private TextMeshProUGUI netReputationText;

    [Header("Updated Totals")]
    [SerializeField] private TextMeshProUGUI totalMoneyText;
    [SerializeField] private TextMeshProUGUI totalReputationText;
    [SerializeField] private TextMeshProUGUI daysCompletedText;

    [Header("Day Timing")]
    [SerializeField] private int startHour = 8;
    [SerializeField] private int endHour = 18;
    [SerializeField] private float realSecondsPerInGameMinute = 1f;

    [Header("Clock Alert")]
    [SerializeField] private float warningShakeDuration = 1f;
    [SerializeField] private float warningShakeStrength = 10f;

    [Header("Game State")]
    [SerializeField] private int currentDay = 1;
    [SerializeField] private int moneyEarnedToday = 0;
    [SerializeField] private int taskRevenueToday = 0;
    [SerializeField] private int tipsEarnedToday = 0;
    [SerializeField] private int taskReputationToday = 0;
    [SerializeField] private int failedTaskPenaltyToday = 0;
    [SerializeField] private int totalTasksToday = 0;
    [SerializeField] private int perfectTasksToday = 0;
    [SerializeField] private int failedTasksToday = 0;

    public event Action<int> OnDayChanged;
    public event Action<int> OnDailyEarningsChanged;
    public event Action<int, int, int> OnDayEnded;
    public event Action<int> OnNextDayStarted;
    public event Action<int, int> OnClockChanged;
    public event Action OnClosingTimeStarted;

    public int CurrentDay => currentDay;
    public int MoneyEarnedToday => moneyEarnedToday;
    public bool IsDayRunning => dayState == DayState.Running;
    public bool IsClosingTime => dayState == DayState.ClosingTime || dayState == DayState.WaitingForTasksToFinish;
    public bool IsReportOpen => dayState == DayState.ReportOpen;

    private GameManager gameManager;
    private ResourceManager resourceManager;
    private TaskSpawner taskSpawner;

    private DayState dayState = DayState.Running;
    private float minuteTimer;
    private int currentHour;
    private int currentMinute;
    private bool oneHourWarningTriggered;
    private Vector2 originalClockAnchoredPosition;

    private enum DayState
    {
        Running,
        ClosingTime,
        WaitingForTasksToFinish,
        ReportOpen
    }

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
        gameManager = GameManager.Instance;
        resourceManager = ResourceManager.Instance;
        taskSpawner = TaskSpawner.Instance;

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (clockRoot != null)
            originalClockAnchoredPosition = clockRoot.anchoredPosition;


        if (startNextDayButton != null)
        {
            startNextDayButton.onClick.RemoveListener(StartNextDay);
            startNextDayButton.onClick.AddListener(StartNextDay);
            startNextDayButton.gameObject.SetActive(false);
        }

        InitializeDayClock();
        NotifyDayStateChanged();
        UpdateUI();
    }

    private void Update()
    {
        if (!ShouldPauseDayClock())
            TickClock();

        if (dayState == DayState.ClosingTime || dayState == DayState.WaitingForTasksToFinish)
            TryFinishDayAfterTasks();
    }

    private bool ShouldPauseDayClock()
    {
        if (dayState != DayState.Running)
            return true;

        if (GameManager.Instance != null && GameManager.Instance.isGamePaused)
            return true;

        if (TaskSpawner.IsResultPopupOpenGlobally)
            return true;

        return false;
    }

    private void InitializeDayClock()
    {
        currentHour = startHour;
        currentMinute = 0;
        minuteTimer = 0f;
        oneHourWarningTriggered = false;
        dayState = DayState.Running;
    }

    private void ResetDailyCounters()
    {
        moneyEarnedToday = 0;
        taskRevenueToday = 0;
        tipsEarnedToday = 0;
        taskReputationToday = 0;
        failedTaskPenaltyToday = 0;
        totalTasksToday = 0;
        perfectTasksToday = 0;
        failedTasksToday = 0;
    }

    public void AddDailyEarnings(int amount)
    {
        moneyEarnedToday += amount;
        taskRevenueToday += amount;
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
    }

    public void AddDailyTips(int amount)
    {
        tipsEarnedToday += amount;
        moneyEarnedToday += amount;
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
    }

    public void RegisterTaskResult(bool wasSuccessful, bool isPerfect, int reputationDelta)
    {
        totalTasksToday++;

        if (wasSuccessful)
        {
            if (isPerfect)
                perfectTasksToday++;
        }
        else
        {
            failedTasksToday++;
        }

        if (reputationDelta > 0)
            taskReputationToday += reputationDelta;
        else if (reputationDelta < 0)
            failedTaskPenaltyToday += Mathf.Abs(reputationDelta);
    }

    public void RegisterFailedTaskPenalty(int penaltyAmount)
    {
        if (penaltyAmount > 0)
            failedTaskPenaltyToday += penaltyAmount;
    }

    private void TickClock()
    {
        if (realSecondsPerInGameMinute <= 0f)
            return;

        minuteTimer += Time.deltaTime;

        while (minuteTimer >= realSecondsPerInGameMinute)
        {
            minuteTimer -= realSecondsPerInGameMinute;
            AdvanceOneMinute();
        }
    }

    private void AdvanceOneMinute()
    {
        currentMinute++;

        if (currentMinute >= 60)
        {
            currentMinute = 0;
            currentHour++;
        }

        OnClockChanged?.Invoke(currentHour, currentMinute);
        UpdateClockUI();

        if (!oneHourWarningTriggered && currentHour == endHour - 1 && currentMinute == 0)
        {
            oneHourWarningTriggered = true;

            if (clockRoot != null)
                StartCoroutine(ClockWarningShakeRoutine());
        }

        if (currentHour >= endHour)
            BeginClosingTime();
    }

    private void BeginClosingTime()
    {
        if (dayState != DayState.Running)
            return;

        dayState = DayState.ClosingTime;

        if (taskSpawner != null)
            taskSpawner.StopSpawning();

        OnClosingTimeStarted?.Invoke();
        TryFinishDayAfterTasks();
    }

    private void TryFinishDayAfterTasks()
    {
        if (taskSpawner == null)
            taskSpawner = TaskSpawner.Instance;

        bool hasActiveTasks = taskSpawner != null && taskSpawner.HasActiveTasks();
        bool hasUnfinishedFlow = TaskFlowManager.Instance != null && TaskFlowManager.Instance.HasUnfinishedRestaurantFlow();

        if (hasActiveTasks || hasUnfinishedFlow)
        {
            dayState = DayState.WaitingForTasksToFinish;
            return;
        }

        EndDay();
    }

    public void EndDay()
    {
        if (dayState == DayState.ReportOpen)
            return;

        dayState = DayState.ReportOpen;

        int totalSalaries = CalculateTotalWages();
        int dailyExpenses = totalSalaries;

        if (resourceManager != null)
            resourceManager.ModifyMoney(-totalSalaries);

        int netProfit = moneyEarnedToday - dailyExpenses;
        int netReputation = taskReputationToday - failedTaskPenaltyToday;

        OnDayEnded?.Invoke(moneyEarnedToday, totalSalaries, netProfit);

        ShowDailyReport(dailyExpenses, netProfit, netReputation);
    }

    private int CalculateTotalWages()
    {
        int total = 0;

        if (EmployeeRosterManager.Instance == null)
            return total;

        IReadOnlyList<EmployeeData> employees = EmployeeRosterManager.Instance.CurrentEmployees;

        for (int i = 0; i < employees.Count; i++)
        {
            EmployeeData employee = employees[i];

            if (employee == null)
                continue;

            total += employee.GetDailyCost();
        }

        return total;
    }

    private void ShowDailyReport(int dailyExpenses, int netProfit, int netReputation)
    {
        float successRate = totalTasksToday > 0
            ? ((float)(totalTasksToday - failedTasksToday) / totalTasksToday) * 100f
            : 0f;

        if (tasksProcessedDetailsText != null)
        {
            tasksProcessedDetailsText.text =
                $"Total Tasks: {totalTasksToday}\n" +
                $"Perfect Tasks: {perfectTasksToday}\n" +
                $"Failed Tasks: {failedTasksToday}";
        }

        if (successRateText != null)
        {
            successRateText.text = $"SUCCESS RATE: {successRate.ToString("F1")}%";
        }

        if (moneySummaryDetailsText != null)
        {
            moneySummaryDetailsText.text =
                $"Task Revenue: {FormatSignedMoney(taskRevenueToday)}\n" +
                $"Tips Earned: {FormatSignedMoney(tipsEarnedToday)}\n" +
                $"Employee Salaries: {FormatSignedMoney(-CalculateTotalWages())}\n" +
                $"Daily Expenses: {FormatSignedMoney(-dailyExpenses)}";
        }

        if (netProfitText != null)
        {
            netProfitText.text = $"NET PROFIT: {FormatSignedMoney(netProfit)}";
        }

        if (reputationSummaryDetailsText != null)
        {
            reputationSummaryDetailsText.text =
                $"Task Reputation: {FormatSignedValue(taskReputationToday)}\n" +
                $"Failed Task Penalty: {FormatSignedValue(-failedTaskPenaltyToday)}";
        }

        if (netReputationText != null)
        {
            netReputationText.text = $"NET REPUTATION: {FormatSignedValue(netReputation)}";
        }

        if (totalMoneyText != null && resourceManager != null)
        {
            totalMoneyText.text = $"${resourceManager.CurrentMoney}";
        }

        if (totalReputationText != null && resourceManager != null)
        {
            totalReputationText.text = $"{resourceManager.CurrentReputation} POINTS";
        }

        if (daysCompletedText != null)
        {
            daysCompletedText.text = $"{currentDay}";
        }

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(true);

        if (startNextDayButton != null)
            startNextDayButton.gameObject.SetActive(true);
    }

    public void StartNextDay()
    {
        if (TryOpenRecruitmentBeforeNextDay())
            return;

        StartNextDayInternal();
    }

    private bool TryOpenRecruitmentBeforeNextDay()
    {
        if (StaffRecruitmentManager.Instance == null)
            return false;

        if (!StaffRecruitmentManager.Instance.ShouldOpenAfterCompletedDay(currentDay))
            return false;

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (startNextDayButton != null)
            startNextDayButton.gameObject.SetActive(false);

        StaffRecruitmentManager.Instance.OpenRecruitment(StartNextDayInternal);
        return true;
    }

    private void StartNextDayInternal()
    {
        currentDay++;
        ResetDailyCounters();
        InitializeDayClock();

        if (dailyReportPanel != null)
            dailyReportPanel.SetActive(false);

        if (taskSpawner == null)
            taskSpawner = TaskSpawner.Instance;

        if (taskSpawner != null)
            taskSpawner.ResetForNewDay();

        if (TaskFlowManager.Instance != null)
            TaskFlowManager.Instance.ResetForNewDay();

        TaskGeneratorStructure[] allStructures = FindObjectsByType<TaskGeneratorStructure>(FindObjectsSortMode.None);

        for (int i = 0; i < allStructures.Length; i++)
        {
            if (allStructures[i] == null)
                continue;

            allStructures[i].ResetForNewDay();
        }

        if (EmployeeRuntimeManager.Instance != null)
            EmployeeRuntimeManager.Instance.ResetEmployeesForNewDay();

        if (EmployeeRosterManager.Instance != null)
            EmployeeRosterManager.Instance.RefreshAllViews();

        if (startNextDayButton != null)
            startNextDayButton.gameObject.SetActive(true);

        if (taskSpawner != null)
            taskSpawner.StartSpawning();

        NotifyDayStateChanged();
        UpdateUI();
    }

    private void NotifyDayStateChanged()
    {
        OnDayChanged?.Invoke(currentDay);
        OnDailyEarningsChanged?.Invoke(moneyEarnedToday);
        OnNextDayStarted?.Invoke(currentDay);
        OnClockChanged?.Invoke(currentHour, currentMinute);
    }

    private void UpdateUI()
    {
        UpdateDayUI();
        UpdateClockUI();
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }

    private void UpdateClockUI()
    {
        if (clockText != null)
            clockText.text = $"{currentHour:00}:{currentMinute:00}";
    }

    private string FormatSignedMoney(int value)
    {
        if (value > 0)
            return WrapWithColor($"+ ${value}", positiveValueColor);

        if (value < 0)
            return WrapWithColor($"- ${Mathf.Abs(value)}", negativeValueColor);

        return WrapWithColor("$0", neutralValueColor);
    }

    private string FormatSignedValue(int value)
    {
        if (value > 0)
            return WrapWithColor($"+ {value}", positiveValueColor);

        if (value < 0)
            return WrapWithColor($"- {Mathf.Abs(value)}", negativeValueColor);

        return WrapWithColor("0", neutralValueColor);
    }

    private string WrapWithColor(string text, Color color)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
    }

    private IEnumerator ClockWarningShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < warningShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float offsetX = UnityEngine.Random.Range(-warningShakeStrength, warningShakeStrength);
            float offsetY = UnityEngine.Random.Range(-warningShakeStrength, warningShakeStrength);

            if (clockRoot != null)
                clockRoot.anchoredPosition = originalClockAnchoredPosition + new Vector2(offsetX, offsetY);

            yield return null;
        }

        if (clockRoot != null)
            clockRoot.anchoredPosition = originalClockAnchoredPosition;
    }
}