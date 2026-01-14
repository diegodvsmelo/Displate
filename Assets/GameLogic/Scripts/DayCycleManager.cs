using UnityEngine;
using TMPro;
using System.Collections.Generic; // Para usar Listas

public class DayCycleManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI dayText;        
    public GameObject dailyReportPanel;    
    public TextMeshProUGUI reportDetailsText; 

    [Header("Game State")]
    public int currentDay = 1;
    public int moneyEarnedToday = 0;

    private GameManager gameManager;
    private ResourceManager resourceManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        resourceManager = FindFirstObjectByType<ResourceManager>();
        
        UpdateDayUI();
        dailyReportPanel.SetActive(false); 
    }

    // Chamado pelo GameManager ou ResourceManager toda vez que ganhamos dinheiro
    public void AddDailyEarnings(int amount)
    {
        moneyEarnedToday += amount;
    }

    // O botão "ENCERRAR DIA" na UI vai chamar isso
    public void EndDay()
    {
        if (gameManager.spawner != null)
        {
            gameManager.spawner.StopSpawning();
        }

        int totalSalaries = CalculateTotalWages();
        
        resourceManager.ModifyMoney(-totalSalaries);

        ShowDailyReport(totalSalaries);
    }

    int CalculateTotalWages()
    {
        int total = 0;
        
        EmployeeCard[] allCards = FindObjectsByType<EmployeeCard>(FindObjectsSortMode.None);
        foreach (EmployeeCard card in allCards)
        {
            if (card.transform.parent != null) 
            {
                total += card.data.GetDailyCost();
            }
        }
        
        return total;
    }

    void ShowDailyReport(int wagesPaid)
    {
        int profit = moneyEarnedToday - wagesPaid;

        string report = $"RESUMO DO DIA {currentDay}\n\n";
        report += $"Faturamento: <color=green>+${moneyEarnedToday}</color>\n";
        report += $"Salários: <color=red>-${wagesPaid}</color>\n";
        report += "----------------\n";
        
        if (profit >= 0)
            report += $"Lucro Líquido: <color=green>${profit}</color>";
        else
            report += $"Prejuízo: <color=red>${profit}</color>";

        reportDetailsText.text = report;
        dailyReportPanel.SetActive(true);
    }

    public void StartNextDay()
    {
        currentDay++;
        moneyEarnedToday = 0;
        UpdateDayUI();

        dailyReportPanel.SetActive(false);

        if (gameManager.spawner != null)
        {
            gameManager.spawner.StartSpawning();
        }
    }

    void UpdateDayUI()
    {
        if (dayText != null) dayText.text = $"Dia {currentDay}";
    }
}