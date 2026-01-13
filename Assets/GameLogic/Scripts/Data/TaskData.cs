using UnityEngine;
using System.Collections.Generic;

public enum TaskCategory
{
    Cooking,
    Service,
    Operational,
    Agility 
}

[CreateAssetMenu(fileName = "New Task", menuName = "Restaurant/Task")]
public class TaskData : ScriptableObject
{
    [Header("Task Info")]
    public string taskName;
    [TextArea] public string description;

    [Header("Constraints")]
    [Tooltip("Quantos funcionários cabem nesta missão?")]
    public int maxSlots = 1; 

    [Header("Requirements")]
    public List<TaskRequirement> requirements; 

    [Header("Difficulty Settings")]
    [Tooltip("Agora este valor representa a PONTUAÇÃO TOTAL necessária para 100%")]
    public float difficultyPoints; 
    
    public float baseDuration; 
    public int staminaCost;

    [Header("Rewards & Penalties")]
    public int moneyReward;       
    public int reputationReward;  
    public int reputationPenalty; 

    [Header("XP Rewards")]
    public int xpOnSuccess;   
    public int xpOnFailure;   
    public int xpOnCritical; //Quando chance for 100%
}

[System.Serializable]
public struct TaskRequirement
{
    public TaskCategory category;
    [Range(0, 100)] public int weight; 
}