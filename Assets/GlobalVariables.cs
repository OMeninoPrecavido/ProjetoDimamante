using UnityEngine;

public static class GlobalVariables
{
    public static int LevelsUnlocked { get; private set; } = 1;
    public static void IncrementLevelsUnlocked() => LevelsUnlocked++;
}
