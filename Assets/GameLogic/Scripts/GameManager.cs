using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject slotPrefab;
    public Transform slotsContainer;
    public Transform rosterContainer; // NOVO: Arraste o container onde ficam os funcionários parados aqui!
    
    public MinigameUI minigameUI; 
    
    [Header("UI Panels")]
    public GameObject mapPanel;
    public GameObject missionPanel;
    public TaskSpawner spawner;

    public TaskData missaoTeste;
    private List<Slot> missionSlots = new List<Slot>(); 
    private List<Slot> rosterSlots = new List<Slot>();
    public int squadSlotNumbers = 6;

    public List<EmployeeData> startingEmployees; 
    public GameObject employeeCardPrefab;
    private bool isMissionRunning = false;
    public ResourceManager resourceManager;
    
    void Start()
    {
        SetupRosterSlots();
        SpawnStartingCrew();
        ShowMap();
    }
    void Update()
    {
        // 1. Verifica se temos uma missão aberta (missaoTeste != null)
        // 2. Verifica se NÃO apertamos o botão dispatch ainda (!isMissionRunning)
        //    (Pois não queremos que a barra mude enquanto o ponteiro está girando)
        if (missaoTeste != null && !isMissionRunning)
        {
            // Pega quem está nos slots AGORA
            List<EmployeeData> currentSquad = GetSquadFromSlots();
            
            // Se tiver gente, calcula e mostra
            if (currentSquad.Count > 0)
            {
                float chance = CalculateSquadChance(missaoTeste, currentSquad);
                minigameUI.SetZoneSize(chance); // <--- Isso atualiza a barra verde ao vivo!
            }
            else
            {
                // Se tirou todo mundo, a barra zera
                minigameUI.SetZoneSize(0);
            }
        }
    }

    public void ShowMap()
    {
        mapPanel.SetActive(true);
        missionPanel.SetActive(false);
        spawner.StartSpawning();
    }

    public void OpenMissionWindow(TaskData task)
    {
        mapPanel.SetActive(false);
        missionPanel.SetActive(true);

        this.missaoTeste = task;
        
        GenerateSlotsForTask(task);
        
        // CORREÇÃO 1: Reseta a UI para o ponteiro voltar a mexer
        minigameUI.ResetUI(); 
        minigameUI.SetZoneSize(0);
    }

    void GenerateSlotsForTask(TaskData task)
    {
        // Limpa slots antigos
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        missionSlots.Clear();

        for (int i = 0; i < task.maxSlots; i++)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, slotsContainer);
            Slot slotScript = newSlotObj.GetComponent<Slot>();
            if (slotScript != null)
            {
                slotScript.isRoster = false;
                missionSlots.Add(slotScript);
            }
        }
    }

    public void OnDispatchButtonPress()
    {
        if (isMissionRunning) return;

        List<EmployeeData> currentSquad = GetSquadFromSlots();
        
        if (currentSquad.Count > 0)
        {
            isMissionRunning = true;
            ConsumeSquadStamina();

            float chance = CalculateSquadChance(missaoTeste, currentSquad);
            minigameUI.SetZoneSize(chance);
            
            // --- NOVA LÓGICA DE CRÍTICO ---
            bool isCritical = chance >= 100f; // Verifica se é 100% garantido
            
            // Passamos essa informação para a Coroutine
            StartCoroutine(ProcessMissionResult(chance, isCritical));
        }
        else
        {
            Debug.LogWarning("Squad vazio!");
        }
    }

    IEnumerator ProcessMissionResult(float chancePercent, bool isCritical)
    {
        // LÓGICA DO TEMPO:
        if (isCritical)
        {
            Debug.Log("CRÍTICO! 100% de chance! Finalização Imediata!");
            // Não espera os 3 segundos. Passa direto.
            // Podemos esperar uma fração de segundo só para não ser glitch visual
            yield return new WaitForSeconds(0.5f); 
        }
        else
        {
            // Fluxo normal: Tensão de 3 segundos
            yield return new WaitForSeconds(3f);
        }

        // Lógica de Sucesso (Se for crítico, é sucesso automático)
        float roll = Random.Range(0f, 100f);
        bool isSuccess = isCritical || (roll <= chancePercent);

        // -- PARAR O PONTEIRO --
        // (Sua lógica visual do ponteiro continua aqui...)
        float zoneWidth = (minigameUI.totalWidth * chancePercent) / 100f;
        if (zoneWidth >= minigameUI.totalWidth) zoneWidth = minigameUI.totalWidth - 1;
        float stopX = isSuccess ? Random.Range(0f, zoneWidth) : Random.Range(zoneWidth, minigameUI.totalWidth);
        minigameUI.StopPointer(stopX);
        
        // -- RECOMPENSAS E XP --
        if (isSuccess)
        {
            Debug.Log("SUCESSO!");
            
            // Dinheiro e Fama
            resourceManager.ModifyMoney(missaoTeste.moneyReward);
            resourceManager.ModifyReputation(missaoTeste.reputationReward);

            // DISTRIBUIÇÃO DE XP (SUCESSO OU CRÍTICO)
            int xpToGive = isCritical ? missaoTeste.xpOnCritical : missaoTeste.xpOnSuccess;
            GiveSquadExperience(xpToGive);
        }
        else
        {
            Debug.Log("FALHA!");
            resourceManager.ModifyReputation(-missaoTeste.reputationPenalty);
            
            // XP DE CONSOLAÇÃO (FALHA)
            GiveSquadExperience(missaoTeste.xpOnFailure);
        }

        yield return new WaitForSeconds(2f);
        ReturnCrewToRoster();
        ShowMap();
        
        missaoTeste = null;
        isMissionRunning = false;
    }

    void GiveSquadExperience(int amount)
    {
        foreach (Slot slot in missionSlots) // Usa a lista missionSlots que já temos
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null)
                {
                    card.AddExperience(amount);
                }
            }
        }
    }


    void ReturnCrewToRoster()
    {
        // Percorre cada slot da MISSÃO para ver se tem alguém lá
        foreach (Slot missionSlot in missionSlots)
        {
            if (missionSlot.transform.childCount > 0)
            {
                // Achamos um funcionário preso na missão
                Transform card = missionSlot.transform.GetChild(0);
                Draggable draggable = card.GetComponent<Draggable>();

                // Agora procuramos uma casa vazia no ROSTER
                Slot emptyRosterSlot = FindFirstEmptyRosterSlot();

                if (emptyRosterSlot != null)
                {
                    // Move a carta para o slot vazio encontrado
                    card.SetParent(emptyRosterSlot.transform);
                    card.localPosition = Vector3.zero; // Centraliza

                    // Atualiza a lógica do Draggable
                    if (draggable != null)
                    {
                        draggable.originalParent = emptyRosterSlot.transform;
                    }
                }
                else
                {
                    Debug.LogError("ERRO: Não há slots vazios no Roster para devolver o funcionário!");
                    // Aqui você poderia destruir a carta ou jogar num limbo temporário
                }
            }
        }
    }
    List<EmployeeData> GetSquadFromSlots()
    {
        List<EmployeeData> squad = new List<EmployeeData>();
        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                if (card != null && card.data != null) squad.Add(card.data);
            }
        }
        return squad;
    }

    public float CalculateSquadChance(TaskData task, List<EmployeeData> squad)
    {
        // (Use a mesma lógica de cálculo que você já tem funcionando)
        // Vou resumir aqui para não ficar gigante, mas mantenha o seu código:
        float totalSquadScore = 0;
        float totalWeights = 0;
        foreach (var req in task.requirements) {
            float attrSum = 0;
            foreach (var mem in squad) {
                if (req.category == TaskCategory.Cooking) attrSum += mem.cookingSkill;
                else if (req.category == TaskCategory.Service) attrSum += mem.serviceSkill;
                else if (req.category == TaskCategory.Operational) attrSum += mem.operationalSkill;
                else if (req.category == TaskCategory.Agility) attrSum += mem.agility;
            }
            totalSquadScore += attrSum * req.weight;
            totalWeights += req.weight;
        }
        if (totalWeights == 0) return 0;
        float finalScore = totalSquadScore / totalWeights;
        return Mathf.Clamp((finalScore / task.difficultyPoints) * 100f, 0f, 100f);
    }

    Slot FindFirstEmptyRosterSlot()
    {
        foreach (Slot slot in rosterSlots)
        {
            // Se o slot não tem filhos, está vazio
            if (slot.transform.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }
    void SetupRosterSlots()
    {
        rosterSlots.Clear();

        // Opção A: Se você já colocou os slots manualmente na Unity dentro do RosterContainer,
        // apenas pegamos eles.
        foreach (Transform child in rosterContainer)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null)
            {
                rosterSlots.Add(slot);
            }
        }

        // Opção B (Recomendada): Se o Roster estiver vazio, criamos X slots (ex: 10)
        if (rosterSlots.Count == 0)
        {
            for (int i = 0; i < squadSlotNumbers; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, rosterContainer);
                Slot slotScript = newSlot.GetComponent<Slot>();

                slotScript.isRoster = true;

                rosterSlots.Add(slotScript);
            }
        }
    }

    void SpawnStartingCrew()
    {
        for (int i = 0; i < startingEmployees.Count; i++)
        {
            if (i < rosterSlots.Count)
            {
                GameObject newCard = Instantiate(employeeCardPrefab, rosterSlots[i].transform);
                
                // MUDANÇA AQUI:
                // Em vez de: newCard.GetComponent<EmployeeCard>().data = startingEmployees[i];
                // Usamos o Setup para garantir que a cor mude:
                newCard.GetComponent<EmployeeCard>().Setup(startingEmployees[i]);
                
                newCard.GetComponent<Draggable>().originalParent = rosterSlots[i].transform;
            }
        }
    }

    void ConsumeSquadStamina()
    {
        int cost = missaoTeste.staminaCost;

        foreach (Slot slot in missionSlots)
        {
            if (slot.transform.childCount > 0)
            {
                // Pega o componente EmployeeCard do objeto visual
                EmployeeCard card = slot.transform.GetChild(0).GetComponent<EmployeeCard>();
                
                if (card != null)
                {
                    card.ConsumeStamina(cost);
                }
            }
        }
    }
}