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

    SerializedProperty healBuffIconProp;
    SerializedProperty healBuffDurationProp;

    void OnEnable()
    {
        rewardTypeProp = serializedObject.FindProperty("rewardType");
        healAmountProp = serializedObject.FindProperty("healAmount");
        speedMultiplierProp = serializedObject.FindProperty("speedMultiplier");
        speedDurationProp = serializedObject.FindProperty("speedDuration");
        additionalDamageAmountProp = serializedObject.FindProperty("additionalDamageAmount");
        chestSpriteProp = serializedObject.FindProperty("chestSprite");

        speedBuffIconProp = serializedObject.FindProperty("speedBuffIcon");
        additionalDamageBuffIconProp = serializedObject.FindProperty("additionalDamageBuffIcon");

        healBuffIconProp = serializedObject.FindProperty("healBuffIcon");
        healBuffDurationProp = serializedObject.FindProperty("healBuffDuration");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(rewardTypeProp);
        switch ((ChestReward.RewardType)rewardTypeProp.enumValueIndex)
        {
            case ChestReward.RewardType.Heal:
                EditorGUILayout.PropertyField(healAmountProp, new GUIContent("Heal Amount"));
                EditorGUILayout.PropertyField(healBuffIconProp, new GUIContent("Heal Buff Icon"));
                EditorGUILayout.PropertyField(healBuffDurationProp, new GUIContent("Heal Buff Duration"));
                break;

            case ChestReward.RewardType.Speed:
                EditorGUILayout.PropertyField(speedMultiplierProp, new GUIContent("Speed Multiplier"));
                EditorGUILayout.PropertyField(speedDurationProp, new GUIContent("Speed Duration"));
                EditorGUILayout.PropertyField(speedBuffIconProp, new GUIContent("Speed Buff Icon"));
                break;

            case ChestReward.RewardType.AdditionalDamage:
                EditorGUILayout.PropertyField(additionalDamageAmountProp, new GUIContent("Additional Damage Amount"));
                EditorGUILayout.PropertyField(additionalDamageBuffIconProp, new GUIContent("Damage Buff Icon"));
                break;
        }

        EditorGUILayout.PropertyField(chestSpriteProp);
        serializedObject.ApplyModifiedProperties();
    }
}