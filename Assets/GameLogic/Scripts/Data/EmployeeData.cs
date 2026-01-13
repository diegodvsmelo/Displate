using UnityEngine;

[CreateAssetMenu(fileName = "New Employee", menuName = "Restaurant/Employee")]
public class EmployeeData : ScriptableObject
{
    [Header("Identity")]
    public string employeeName;
    public Sprite profilePicture;


    [Header("Visual Settings")]
    public Color cardColor;


    [Header("Skills (0-100)")]
    [Range(0, 100)] public int cookingSkill;     
    [Range(0, 100)] public int serviceSkill;     
    [Range(0, 100)] public int operationalSkill; 


    [Header("Physical Stats")]
    [Range(0, 100)] public int agility;      
    [Range(0, 100)] public int maxStamina;   


    [Header("Progression")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int skillPoints = 0; 
    
    
    // nível 1 precisa de 100, Nível 2 precisa de 200...
    public int GetXpToNextLevel()
    {
        return currentLevel * 100; 
    }
}