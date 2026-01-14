using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSheetUI : MonoBehaviour
{
    [Header("Header Info")]
    public TextMeshProUGUI nameText;
    // public TextMeshProUGUI descriptionText; 
    public TextMeshProUGUI pointsAvailableText;

    [Header("Stat Rows")]
    public StatRowUI cookingRow;
    public StatRowUI serviceRow;
    public StatRowUI operationalRow;
    public StatRowUI agilityRow;

    private EmployeeData currentData;
    
    // Callback: Quem eu devo avisar quando fechar?
    private System.Action onUpdateCallback;

    // Variáveis temporárias
    private int tempPoints;
    private int tempCooking, tempService, tempOperational, tempAgility;

    // MUDANÇA: Adicionado parâmetro opcional 'onUpdate'
    public void OpenSheet(EmployeeData data, System.Action onUpdate = null)
    {
        currentData = data;
        onUpdateCallback = onUpdate; // Guarda a referência
        
        gameObject.SetActive(true);

        // Copia valores
        tempPoints = data.skillPoints;
        tempCooking = data.cookingSkill;
        tempService = data.serviceSkill;
        tempOperational = data.operationalSkill;
        tempAgility = data.agility;

        nameText.text = data.employeeName;
        // if(descriptionText) descriptionText.text = data.description;

        UpdateUI();
    }

    public void ModifyStat(string statName, int change)
    {
        if (change > 0 && tempPoints < change) return;

        if (change < 0)
        {
            if (statName == "cooking" && tempCooking <= currentData.cookingSkill) return;
            if (statName == "service" && tempService <= currentData.serviceSkill) return;
            if (statName == "operational" && tempOperational <= currentData.operationalSkill) return;
            if (statName == "agility" && tempAgility <= currentData.agility) return;
        }

        if (statName == "cooking") tempCooking += change;
        else if (statName == "service") tempService += change;
        else if (statName == "operational") tempOperational += change;
        else if (statName == "agility") tempAgility += change;

        tempPoints -= change;
        UpdateUI();
    }

    void UpdateUI()
    {
        pointsAvailableText.text = $"Pontos Disponíveis: {tempPoints}";

        cookingRow.UpdateVisuals(tempCooking);
        serviceRow.UpdateVisuals(tempService);
        operationalRow.UpdateVisuals(tempOperational);
        agilityRow.UpdateVisuals(tempAgility);
    }

    public void ConfirmChanges()
    {
        // Salva definitivo
        currentData.cookingSkill = tempCooking;
        currentData.serviceSkill = tempService;
        currentData.operationalSkill = tempOperational;
        currentData.agility = tempAgility;
        currentData.skillPoints = tempPoints;

        // MUDANÇA: Avisa o card para desligar o ícone (pois skillPoints agora pode ser 0)
        if (onUpdateCallback != null)
        {
            onUpdateCallback.Invoke();
        }

        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class StatRowUI
{
    public TextMeshProUGUI valueText;
    public void UpdateVisuals(float value)
    {
        valueText.text = value.ToString();
    }
}