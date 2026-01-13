using UnityEngine;
using UnityEngine.UI;

public class EmployeeCard : MonoBehaviour
{
    public EmployeeData data; 
    public Image backgroundImage;

    [Header("Stamina System")]
    public Slider staminaSlider; // Arraste o Slider aqui no Inspector
    public float currentStamina; // Energia atual desta carta específica
    public float staminaRegen=2f;

    void Update()
    {
        // Se a carta está parada (tem um pai)
        if (transform.parent != null)
        {
            // Verifica se o pai é um Slot do Roster (Dica: Podemos checar pelo script Slot)
            Slot mySlot = transform.parent.GetComponent<Slot>();
            
            // Se estou num slot, e meu slot NÃO ESTÁ na lista de slots da missão atual...
            // (Essa é uma verificação simples, num jogo complexo usaríamos Tags ou Estados)
            // Vamos simplificar: Se não estou sendo arrastado, recupero um pouquinho.
            
            if (mySlot != null && mySlot.isRoster)
            {
                RecoverStamina(Time.deltaTime * staminaRegen); 
            }
        }
    }
    public void Setup(EmployeeData newData)
    {
        this.data = newData;

        // SEGURO: Se a referência estiver vazia, tenta pegar agora
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        // Se conseguiu pegar a imagem, aplica a cor
        if (backgroundImage != null)
        {
            backgroundImage.color = newData.cardColor;
        }
        else
        {
            Debug.LogError("ERRO: Não encontrei o componente Image no Card!");
        }

        currentStamina = newData.maxStamina;
        UpdateStaminaUI();
    }

    public void ConsumeStamina(int amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;
        
        UpdateStaminaUI();
    }

    // Função para recuperar energia (Regeneração passiva futura)
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
}