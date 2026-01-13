using UnityEngine;

public class StatButton : MonoBehaviour
{
    [Header("Configuração")]
    public string statName; // Ex: "cooking"
    public int value;       // Ex: 1 (para aumentar) ou -1 (para diminuir)
    
    // O script tenta achar o gerente automaticamente, mas você pode arrastar se quiser
    private CharacterSheetUI manager;

    void Start()
    {
        // Procura o gerente na cena (mesmo que o painel comece desativado)
        manager = FindFirstObjectByType<CharacterSheetUI>(FindObjectsInactive.Include);
    }

    // ESSA função não tem parâmetros, então o Botão vai aceitar!
    public void OnClick()
    {
        if (manager != null)
        {
            manager.ModifyStat(statName, value);
        }
        else
        {
            Debug.LogError("StatButton não encontrou o CharacterSheetUI!");
        }
    }
}