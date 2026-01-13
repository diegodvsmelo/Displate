using UnityEngine;
using TMPro; // Necessário para usar textos modernos

public class ResourceManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI reputationText;

    [Header("Starting Values")]
    public int currentMoney = 100;
    public int currentReputation = 50;

    void Start()
    {
        UpdateUI();
    }

    public void ModifyMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    public void ModifyReputation(int amount)
    {
        currentReputation += amount;
        
        if (currentReputation < 0) currentReputation = 0;
        
        // (Futuramente: Se chegar a 0 -> Game Over)
        
        UpdateUI();
    }

    void UpdateUI()
    {
        // Atualiza os textos na tela
        if (moneyText != null) moneyText.text = $"$: {currentMoney}";
        if (reputationText != null) reputationText.text = $"*: {currentReputation}";
    }
}