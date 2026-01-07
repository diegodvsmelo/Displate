using UnityEngine;

[CreateAssetMenu(fileName = "New Employee", menuName = "Restaurant/Employee")]
public class EmployeeData : ScriptableObject
{
    [Header("Identity")]
    public string employeeName;
    public Sprite profilePicture;

    [Header("Skills (0-100)")]
    [Range(0, 100)] public int cookingSkill;     
    [Range(0, 100)] public int serviceSkill;     
    [Range(0, 100)] public int operationalSkill; 

    [Header("Physical Stats")]
    [Range(0, 100)] public int agility;      
    [Range(0, 100)] public int maxStamina;   
}