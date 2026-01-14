using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EmployeeCard : MonoBehaviour, IPointerClickHandler
{
    public EmployeeData data; 
    public Image backgroundImage;

    [Header("Stamina System")]
    public Slider staminaSlider;
    public float currentStamina;
    public float staminaRegen = 2f;

    [Header("Level & Upgrade")]
    public TextMeshProUGUI levelText;
    public Slider xpSlider;
    public GameObject upgradeIcon; // O ícone de exclamação

    // Referência para o painel de ficha de personagem
    private CharacterSheetUI characterSheet;

    void Start()
    {
        characterSheet = FindFirstObjectByType<CharacterSheetUI>(FindObjectsInactive.Include);
    }

    void Update()
    {
        if (transform.parent != null)
        {
            Slot mySlot = transform.parent.GetComponent<Slot>();
            
            // Só regenera se for um slot do Roster
            if (mySlot != null && mySlot.isRoster)
            {
                RecoverStamina(Time.deltaTime * staminaRegen); 
            }
        }
    }

    // --- INTERAÇÃO CORRIGIDA ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (characterSheet != null)
            {
                // MUDANÇA: Passamos 'UpdateLevelUI' como callback
                // Assim, quando fechar o painel, ele avisa este card para se atualizar
                characterSheet.OpenSheet(data, UpdateLevelUI);
            }
            else
            {
                Debug.LogWarning("Painel CharacterSheetUI não encontrado!");
            }
        }
    }
    // ---------------------------

    public void Setup(EmployeeData newData)
    {
        this.data = newData;

        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (backgroundImage != null) backgroundImage.color = newData.cardColor;

        currentStamina = newData.maxStamina;
        UpdateStaminaUI();
        UpdateLevelUI(); 
    }

    public void ConsumeStamina(int amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;
        UpdateStaminaUI();
    }

    public void RecoverStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina > data.maxStamina) currentStamina = data.maxStamina;
        UpdateStaminaUI();
    }

    void UpdateStaminaUI()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = data.maxStamina;
            staminaSlider.value = currentStamina;
        }
    }

    public void AddExperience(int amount)
    {
        data.currentXP += amount;
        // Debug.Log($"{data.employeeName} ganhou {amount} XP!");

        CheckLevelUp();
        UpdateLevelUI();
    }

    void CheckLevelUp()
    {
        while (data.currentXP >= data.GetXpToNextLevel())
        {
            data.currentXP -= data.GetXpToNextLevel();
            data.currentLevel++;
            data.skillPoints+=5; // Ganha ponto 
            
            Debug.Log($"LEVEL UP! {data.employeeName} nv. {data.currentLevel}. Pontos: {data.skillPoints}");
        }
    }

    // Chamado no Setup, no LevelUp e AGORA chamado também quando o Painel fecha
    public void UpdateLevelUI()
    {
        if (levelText != null) levelText.text = $"Nv. {data.currentLevel}";

        if (xpSlider != null)
        {
            xpSlider.maxValue = data.GetXpToNextLevel();
            xpSlider.value = data.currentXP;
        }

        // LÓGICA DO ÍCONE
        if (upgradeIcon != null)
        {
            bool hasPoints = data.skillPoints > 0;
            
            // Só ativa/desativa se o estado mudou para economizar processamento
            if (upgradeIcon.activeSelf != hasPoints)
            {
                upgradeIcon.SetActive(hasPoints);
            }
        }
    }
}