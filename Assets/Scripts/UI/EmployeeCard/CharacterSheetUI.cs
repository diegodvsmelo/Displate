using TMPro;
using UnityEngine;
using System;

public class CharacterSheetUI : MonoBehaviour
{
    [Header("Header Info")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI pointsAvailableText;

    [Header("Stat Rows")]
    public StatRowUI cookingRow;
    public StatRowUI serviceRow;
    public StatRowUI operationalRow;
    public StatRowUI agilityRow;

    // OBSERVERS
    public event Action<EmployeeData> OnSheetOpened;
    public event Action<EmployeeData> OnSheetConfirmed;
    public event Action<EmployeeData> OnSheetClosedWithoutSaving;
    public event Action<EmployeeData> OnTempStatsChanged;

    private EmployeeData currentData;
    private Action onUpdateCallback;


    private int tempPoints;
    private int tempCooking;
    private int tempService;
    private int tempOperational;
    private int tempAgility;

    private void Awake()
    {
        
    }

    public void OpenSheet(EmployeeData data, Action onUpdate = null)
    {
        currentData = data;
        onUpdateCallback = onUpdate;

        gameObject.SetActive(true);
        GameManager.Instance.PauseGame();

        tempPoints = data.skillPoints;
        tempCooking = data.cookingSkill;
        tempService = data.serviceSkill;
        tempOperational = data.operationalSkill;
        tempAgility = data.agility;

        if (nameText != null)
            nameText.text = data.employeeName;

        UpdateUI();

        OnSheetOpened?.Invoke(currentData);
    }

    public void ModifyStat(string statName, int change)
    {
        if (change > 0 && tempPoints < change)
            return;

        if (change < 0)
        {
            if (statName == "cooking" && tempCooking <= currentData.cookingSkill) return;
            if (statName == "service" && tempService <= currentData.serviceSkill) return;
            if (statName == "operational" && tempOperational <= currentData.operationalSkill) return;
            if (statName == "agility" && tempAgility <= currentData.agility) return;
        }

        if (statName == "cooking")
            tempCooking += change;
        else if (statName == "service")
            tempService += change;
        else if (statName == "operational")
            tempOperational += change;
        else if (statName == "agility")
            tempAgility += change;

        tempPoints -= change;

        UpdateUI();

        OnTempStatsChanged?.Invoke(currentData);
    }

    private void UpdateUI()
    {
        if (pointsAvailableText != null)
            pointsAvailableText.text = $"Pontos Disponíveis: {tempPoints}";

        cookingRow.UpdateVisuals(tempCooking);
        serviceRow.UpdateVisuals(tempService);
        operationalRow.UpdateVisuals(tempOperational);
        agilityRow.UpdateVisuals(tempAgility);
    }

    public void ConfirmChanges()
    {
        currentData.cookingSkill = tempCooking;
        currentData.serviceSkill = tempService;
        currentData.operationalSkill = tempOperational;
        currentData.agility = tempAgility;
        currentData.skillPoints = tempPoints;

        onUpdateCallback?.Invoke();

        OnSheetConfirmed?.Invoke(currentData);

        GameManager.Instance.ResumeGame();

        gameObject.SetActive(false);
    }

    public void CloseWithoutSaving()
    {
        OnSheetClosedWithoutSaving?.Invoke(currentData);

        GameManager.Instance.ResumeGame();

        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class StatRowUI
{
    public TextMeshProUGUI valueText;

    public void UpdateVisuals(float value)
    {
        if (valueText != null)
            valueText.text = value.ToString();
    }
}