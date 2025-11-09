using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [Header("Objectives (Max 6)")]
    public QuestObjective[] objectives = new QuestObjective[6];

    [Header("Appearance")]
    public Color inProgressColor = Color.white;
    public Color completedColor = Color.green;

    private void Start()
    {
        ResetAllObjectives();
    }

    public void UpdateObjective(int index, int newAmount)
    {
        if (index < 0 || index >= objectives.Length)
        {
            Debug.LogWarning($"[QuestTracker] Invalid objective index {index}");
            return;
        }

        QuestObjective obj = objectives[index];
        obj.currentAmount = Mathf.Clamp(newAmount, 0, obj.targetAmount);

        if (obj.uiText != null)
        {
            bool completed = obj.currentAmount >= obj.targetAmount;
            obj.uiText.text = $"{obj.description} {obj.currentAmount}/{obj.targetAmount}";

            bool shouldShow = !string.IsNullOrEmpty(obj.description) && obj.targetAmount > 0;
            obj.uiText.color = completed ? completedColor : inProgressColor;
            obj.uiText.gameObject.SetActive(shouldShow);
        }
    }

    public void ResetAllObjectives()
    {
        foreach (var obj in objectives)
        {
            obj.currentAmount = 0;
            if (obj.uiText != null)
            {
                obj.uiText.text = $"{obj.description} 0/{obj.targetAmount}";

                bool shouldShow = !string.IsNullOrEmpty(obj.description) && obj.targetAmount > 0;
                obj.uiText.color = inProgressColor;
                obj.uiText.gameObject.SetActive(shouldShow);
            }
        }
    }
}