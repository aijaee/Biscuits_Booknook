using TMPro;
using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public string description;
    public int currentAmount;
    public int targetAmount;
    public TextMeshProUGUI uiText;
}