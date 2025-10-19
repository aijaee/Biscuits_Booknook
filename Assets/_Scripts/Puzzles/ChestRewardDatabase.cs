using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Puzzles/ChestRewardDatabase")]
public class ChestRewardDatabase : ScriptableObject
{
    [System.Serializable]
    public class RewardEntry
    {
        public ChestReward reward;
        [Range(0, 100)]
        public int dropRatePercentage;
    }

    public List<RewardEntry> rewardEntries;
}