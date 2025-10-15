using UnityEngine;

public static class GameProgress
{
    public static bool Level1Completed
    {
        get => PlayerPrefs.GetInt("Level1Completed", 0) == 1;
        set => PlayerPrefs.SetInt("Level1Completed", value ? 1 : 0);
    }
}
