using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChestReward))]
public class ChestRewardEditor : Editor
{
    SerializedProperty rewardTypeProp;
    SerializedProperty healAmountProp;
    SerializedProperty speedMultiplierProp;
    SerializedProperty speedDurationProp;
    SerializedProperty additionalDamageAmountProp;
    SerializedProperty chestSpriteProp;
    SerializedProperty speedBuffIconProp;
    SerializedProperty additionalDamageBuffIconProp;

    void OnEnable()
    {
        rewardTypeProp = serializedObject.FindProperty("rewardType");
        healAmountProp = serializedObject.FindProperty("healAmount");
        speedMultiplierProp = serializedObject.FindProperty("speedMultiplier");
        speedDurationProp = serializedObject.FindProperty("speedDuration");
        additionalDamageAmountProp = serializedObject.FindProperty("additionalDamageAmount");
        chestSpriteProp = serializedObject.FindProperty("chestSprite");

        // find the new icon fields
        speedBuffIconProp = serializedObject.FindProperty("speedBuffIcon");
        additionalDamageBuffIconProp = serializedObject.FindProperty("additionalDamageBuffIcon");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rewardTypeProp);
        switch ((ChestReward.RewardType)rewardTypeProp.enumValueIndex)
        {
            case ChestReward.RewardType.Heal:
                EditorGUILayout.PropertyField(healAmountProp, new GUIContent("Heal Amount"));
                break;

            case ChestReward.RewardType.Speed:
                EditorGUILayout.PropertyField(speedMultiplierProp, new GUIContent("Speed Multiplier"));
                EditorGUILayout.PropertyField(speedDurationProp, new GUIContent("Speed Duration"));
                // draw speed icon
                EditorGUILayout.PropertyField(speedBuffIconProp, new GUIContent("Speed Buff Icon"));
                break;

            case ChestReward.RewardType.AdditionalDamage:
                EditorGUILayout.PropertyField(additionalDamageAmountProp, new GUIContent("Additional Damage Amount"));
                // draw damage icon
                EditorGUILayout.PropertyField(additionalDamageBuffIconProp, new GUIContent("Damage Buff Icon"));
                break;
        }

        EditorGUILayout.PropertyField(chestSpriteProp);
        serializedObject.ApplyModifiedProperties();
    }
}