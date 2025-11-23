using UnityEngine;

public enum UpgradeType
{
    None,
    AddStrongAttack,
    ReduceDashCooldown
}

[CreateAssetMenu(fileName = "NewPlayerUpgrade", menuName = "Upgrades/PlayerUpgrade")]
public class PlayerUpgrade : ScriptableObject
{
    public UpgradeType upgradeType;
    public float value;
}