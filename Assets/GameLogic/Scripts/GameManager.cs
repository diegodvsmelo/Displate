using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Variáveis temporárias só para teste (já que não temos UI ainda)
    // Você vai arrastar seus assets para cá no Inspector!
    public List<EmployeeData> equipeTeste;
    public TaskData missaoTeste;

    public MinigameUI minigameUI;
    void Start()
    {
        // Ao dar Play, ele roda o teste automaticamente
        if (missaoTeste != null && equipeTeste != null && equipeTeste.Count > 0)
        {
            float chance = CalculateSquadChance(missaoTeste, equipeTeste);
            Debug.Log($"Chance da Equipe: {chance}%");

            minigameUI.SetZoneSize(chance);
            StartCoroutine(ProcessMissionResult(chance));
        }
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
}