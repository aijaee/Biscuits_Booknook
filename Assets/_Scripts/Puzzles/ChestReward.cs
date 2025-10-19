using UnityEngine;

[CreateAssetMenu(menuName = "Puzzles/ChestReward")]
public class ChestReward : ScriptableObject
{
    public enum RewardType { Heal, Speed, AdditionalDamage }

    public RewardType rewardType;
    public int healAmount;
    public float speedMultiplier;
    public float speedDuration;
    public int additionalDamageAmount;
    public Sprite chestSprite;
    public Sprite speedBuffIcon;
    public Sprite additionalDamageBuffIcon;
}