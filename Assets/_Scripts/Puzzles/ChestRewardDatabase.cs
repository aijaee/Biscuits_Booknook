using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Puzzles/ChestRewardDatabase")]
public class ChestRewardDatabase : ScriptableObject
{
    public List<ChestReward> rewards;
}