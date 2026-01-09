using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Variáveis temporárias só para teste (já que não temos UI ainda)
    // Você vai arrastar seus assets para cá no Inspector!
    [Header("Game Data")]
    public TaskData missaoTeste;
    private List<Slot> missionSlots = new List<Slot>();

    [Header("UI References")]
    public GameObject slotPrefab;     
    public Transform slotsContainer;  
    public MinigameUI minigameUI;
    [Header("UI Panels")]
    public GameObject mapPanel;      // O painel novo de Spawn
    public GameObject missionPanel;  // O painel antigo de Slots/Arrastar
    public TaskSpawner spawner;      // Referência ao script acima

    private bool isMissionRunning = false;
    void Start()
    {
        ShowMap();
        if (missaoTeste != null)
        {
            // 1. Gera os slots visuais antes de qualquer coisa
            GenerateSlotsForTask(missaoTeste);
        }
    }
    public void ShowMap()
    {
        mapPanel.SetActive(true);
        missionPanel.SetActive(false);
        
        // Começa a gerar tasks
        spawner.StartSpawning();
    }
    public void OpenMissionWindow(TaskData task)
    {
        minigameUI.ResetUI();
        // Troca os painéis
        mapPanel.SetActive(false);
        missionPanel.SetActive(true);

        // Configura a missão selecionada
        this.missaoTeste = task; // Atualiza a variável que já usávamos
        
        // Gera os slots visuais para essa nova missão (Lógica que já criamos antes)
        GenerateSlotsForTask(task);
        
        // Reseta a UI do minigame
        minigameUI.SetZoneSize(0);
    }
    private void Update()
    {
        // MUDANÇA 2: Vamos calcular em tempo real (para ver a barra mexendo enquanto arrasta)
        // Em um jogo final, faríamos isso com eventos para economizar processamento,
        // mas para protótipo, o Update funciona bem.
        
        if (missaoTeste != null&& !isMissionRunning)
        {
            List<EmployeeData> currentSquad = GetSquadFromSlots();
            
            // Só calcula se tiver alguém na equipe
            if (currentSquad.Count > 0)
            {
                float chance = CalculateSquadChance(missaoTeste, currentSquad);
                minigameUI.SetZoneSize(chance); // Atualiza a barra visualmente
            }
            else
            {
                minigameUI.SetZoneSize(0); // Ninguém na missão = 0% chance
            }
        }
    }
    void GenerateSlotsForTask(TaskData task)
    {
        // Limpa slots antigos (caso você troque de missão no futuro)
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        missionSlots.Clear();

        // Cria a quantidade certa de slots
        for (int i = 0; i < task.maxSlots; i++)
        {
            // Cria o objeto visual dentro do container
            GameObject newSlotObj = Instantiate(slotPrefab, slotsContainer);
            
            // Pega o script do slot e guarda na nossa lista lógica
            Slot slotScript = newSlotObj.GetComponent<Slot>();
            if (slotScript != null)
            {
                missionSlots.Add(slotScript);
            }
        }
    }

    // NOVO: Função que vasculha os slots e monta a equipe
    List<EmployeeData> GetSquadFromSlots()
    {
        List<EmployeeData> squad = new List<EmployeeData>();

        // Percorre a lista que acabamos de criar
        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null && card.data != null)
                {
                    squad.Add(card.data);
                }
            }
        }
        return squad;
    }
    IEnumerator ProcessMissionResult(float chancePercent)
    {
        yield return new WaitForSeconds(3f);

        // -- O CÁLCULO DO RNG --
        // Sorteia um número de 0 a 100
        float roll = Random.Range(0f, 100f);
        bool isSuccess = roll <= chancePercent;

        Debug.Log($"Rolagem: {roll} vs Chance: {chancePercent} -> Resultado: {(isSuccess ? "SUCESSO" : "FALHA")}");

        // -- O TRUQUE VISUAL --
        // Precisamos decidir onde o ponteiro vai parar visualmente
        float stopPositionX;
        float totalWidth = minigameUI.totalWidth; // 400
        float zoneWidth = (totalWidth * chancePercent) / 100f; // Tamanho da área verde em pixels

        if (isSuccess)
        {
            stopPositionX = Random.Range(0f, zoneWidth);
        }
        else
        {
            stopPositionX = Random.Range(zoneWidth, totalWidth);
        }

        minigameUI.StopPointer(stopPositionX);
        // Opcional: Depois de parar, pode resetar o 'isMissionRunning' 
        // ou mostrar uma tela de vitória. Por enquanto, o ponteiro só para.
        yield return new WaitForSeconds(2f); // Espera um pouco para ler o resultado

        // VOLTA PARA O MAPA
        ShowMap();
        
        // Limpa a missão atual para evitar erros
        missaoTeste = null;
        isMissionRunning = false;
    }
    

    public float CalculateSquadChance(TaskData task, List<EmployeeData> squad)
    {
        float totalSquadScore = 0;
        float totalWeights = 0;

        // 1. Itera sobre cada requisito da missão (ex: Agilidade e Culinária)
        foreach (var req in task.requirements)
        {
            float attributeSum = 0;

            // 2. Soma o atributo de TODOS os membros da equipe
            foreach (var member in squad)
            {
                switch (req.category)
                {
                    case TaskCategory.Cooking:
                        attributeSum += member.cookingSkill;
                        break;
                    case TaskCategory.Service:
                        attributeSum += member.serviceSkill;
                        break;
                    case TaskCategory.Operational:
                        attributeSum += member.operationalSkill;
                        break;
                    case TaskCategory.Agility:
                        attributeSum += member.agility;
                        break;
                }
            }
            
            totalSquadScore += attributeSum * req.weight;
            totalWeights += req.weight;
        }

        //Evita divisão por zero no caso de falta de configuração dos pesos
        if (totalWeights == 0) return 0;
        
        // Normalizamos a pontuação pelo peso total para manter consistência
        float finalScore = totalSquadScore / totalWeights;

        // Calculamos a porcentagem baseada no alvo da missão
        // Ex: Conseguimos 120 pontos. A missão pede 100. Resultado = 120% (Max 100)
        // Ex: Conseguimos 50 pontos. A missão pede 100. Resultado = 50%.
        
        float chancePercentage = (finalScore / task.difficultyPoints) * 100f;

        // Garante que não passe de 100% (ou deixa passar se quiser "Crítico")
        return Mathf.Clamp(chancePercentage, 0f, 100f);
    }
    public void OnDispatchButtonPress()
    {
        if (isMissionRunning) return; // Evita clicar duas vezes

        List<EmployeeData> currentSquad = GetSquadFromSlots();
        
        // Só deixa começar se tiver alguém na equipe
        if (currentSquad.Count > 0)
        {
            isMissionRunning = true; // Trava o update
            
            float chance = CalculateSquadChance(missaoTeste, currentSquad);
            Debug.Log($"DISPATCH! Chance Final: {chance}%");

            // Inicia a sequência de suspense
            StartCoroutine(ProcessMissionResult(chance));
        }
        else
        {
            Debug.LogWarning("Não pode enviar missão vazia!");
        }
    }

    public void OnRetryButtonPress()
    {
        // 1. Destrava a lógica
        isMissionRunning = false;

        // 2. Manda a UI acordar
        minigameUI.ResetUI();

        Debug.Log("Missão reiniciada! Pode alterar a equipe agora.");
    }

    
}