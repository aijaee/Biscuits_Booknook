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

    void OnEnable()
    {
        rewardTypeProp = serializedObject.FindProperty("rewardType");
        healAmountProp = serializedObject.FindProperty("healAmount");
        speedMultiplierProp = serializedObject.FindProperty("speedMultiplier");
        speedDurationProp = serializedObject.FindProperty("speedDuration");
        additionalDamageAmountProp = serializedObject.FindProperty("additionalDamageAmount");
        chestSpriteProp = serializedObject.FindProperty("chestSprite");
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
                break;
            case ChestReward.RewardType.AdditionalDamage:
                EditorGUILayout.PropertyField(additionalDamageAmountProp, new GUIContent("Additional Damage Amount"));
                break;
        }

        EditorGUILayout.PropertyField(chestSpriteProp);

        serializedObject.ApplyModifiedProperties();
    }
}