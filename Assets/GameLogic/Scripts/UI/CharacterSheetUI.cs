using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSheetUI : MonoBehaviour
{
    [Header("Header Info")]
    public TextMeshProUGUI nameText;
    //public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI pointsAvailableText;

    [Header("Stat Rows (Referências Manuais para protótipo)")]
    // Cada 'StatRow' é uma classe auxiliar simples que definiremos abaixo
    public StatRowUI cookingRow;
    public StatRowUI serviceRow;
    public StatRowUI operationalRow;
    public StatRowUI agilityRow;

    private EmployeeData currentData;
    
    // Variáveis temporárias para não salvar direto no disco enquanto edita
    private int tempPoints;
    private int tempCooking, tempService, tempOperational, tempAgility;

    // Abre a tela preenchendo os dados
    public void OpenSheet(EmployeeData data)
    {
        currentData = data;
        gameObject.SetActive(true);

        // Copia os valores atuais para temporários
        tempPoints = data.skillPoints;
        tempCooking = data.cookingSkill;
        tempService = data.serviceSkill;
        tempOperational = data.operationalSkill;
        tempAgility = data.agility;

        nameText.text = data.employeeName;
        //descriptionText.text = data.description;

        UpdateUI();
    }

    // Chamado pelos botões de + e - (configuraremos na Unity)
    // statName: "cooking", "service", etc.
    // change: +1 ou -1
    public void ModifyStat(string statName, int change)
    {
        // Se for aumentar (+1), precisa ter pontos
        if (change > 0 && tempPoints <= 0) return;

        // Se for diminuir (-1), aplica a lógica de proteção
        if (change < 0)
        {
            if (statName == "cooking" && tempCooking <= currentData.cookingSkill) return;
            if (statName == "service" && tempService <= currentData.serviceSkill) return;
            if (statName == "operational" && tempOperational <= currentData.operationalSkill) return;
            if (statName == "agility" && tempAgility <= currentData.agility) return;
        }

        // Aplica a mudança
        if (statName == "cooking") tempCooking += change;
        else if (statName == "service") tempService += change;
        else if (statName == "operational") tempOperational += change;
        else if (statName == "agility") tempAgility += change;

        // Ajusta o saldo de pontos (se aumentou status, gasta ponto. Se diminuiu, recupera
        tempPoints -= change;

        UpdateUI();
    }

    void UpdateUI()
    {
        pointsAvailableText.text = $"Pontos Disponíveis: {tempPoints}";

        // Atualiza cada linha visualmente
        cookingRow.UpdateVisuals(tempCooking);
        serviceRow.UpdateVisuals(tempService);
        operationalRow.UpdateVisuals(tempOperational);
        agilityRow.UpdateVisuals(tempAgility);
    }

    public void ConfirmChanges()
    {
        // Salva definitivo no ScriptableObject
        currentData.cookingSkill = tempCooking;
        currentData.serviceSkill = tempService;
        currentData.operationalSkill = tempOperational;
        currentData.agility = tempAgility;
        currentData.skillPoints = tempPoints;

        gameObject.SetActive(false);
    }
}

// Classe auxiliar para facilitar o controle das linhas
[System.Serializable]
public class StatRowUI
{
    public TextMeshProUGUI valueText;
    // Aqui você pode adicionar referências aos botões se quiser desativá-los visualmente
    
    public void UpdateVisuals(float value)
    {
        valueText.text = value.ToString();
    }
}