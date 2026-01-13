using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // NECESSÁRIO PARA O CLIQUE

public class EmployeeCard : MonoBehaviour, IPointerClickHandler // INTERFACE ADICIONADA
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
    public GameObject upgradeIcon; // O ícone de exclamação/seta verde

    // Referência para o painel de ficha de personagem
    private CharacterSheetUI characterSheet;

    void Start()
    {
        // Encontra o painel na cena, mesmo se estiver desligado (FindObjectsInactive.Include)
        characterSheet = FindFirstObjectByType<CharacterSheetUI>(FindObjectsInactive.Include);
    }

    void Update()
    {
        // Se a carta está parada (tem um pai)
        if (transform.parent != null)
        {
            Slot mySlot = transform.parent.GetComponent<Slot>();
            
            // Só regenera se for um slot do Roster (Banco de Reservas)
            if (mySlot != null && mySlot.isRoster)
            {
                RecoverStamina(Time.deltaTime * staminaRegen); 
            }
        }
    }

    // --- INTERAÇÃO (PASSO 5) ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // Verifica se foi clique com botão DIREITO
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (characterSheet != null)
            {
                characterSheet.OpenSheet(data);
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

        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = newData.cardColor;
        }

        currentStamina = newData.maxStamina;
        UpdateStaminaUI();
        UpdateLevelUI(); // Isso também vai atualizar o ícone de upgrade
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
        Debug.Log($"{data.employeeName} ganhou {amount} XP!");

        CheckLevelUp();
        UpdateLevelUI();
    }

    void CheckLevelUp()
    {
        while (data.currentXP >= data.GetXpToNextLevel())
        {
            data.currentXP -= data.GetXpToNextLevel();
            data.currentLevel++;
            data.skillPoints++;
            
            Debug.Log($"LEVEL UP! {data.employeeName} nv. {data.currentLevel}. Pontos: {data.skillPoints}");
        }
    }

    public void UpdateLevelUI()
    {
        // Atualiza Texto
        if (levelText != null)
        {
            levelText.text = $"Nv. {data.currentLevel}";
        }

        // Atualiza Slider de XP
        if (xpSlider != null)
        {
            xpSlider.maxValue = data.GetXpToNextLevel();
            xpSlider.value = data.currentXP;
        }

        // --- NOVO: Atualiza Ícone de Upgrade ---
        if (upgradeIcon != null)
        {
            // Mostra o ícone se tiver pontos sobrando (maior que 0)
            upgradeIcon.SetActive(data.skillPoints > 0);
        }
    }
}