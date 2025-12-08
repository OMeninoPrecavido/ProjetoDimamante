using UnityEngine;

public static class GlobalVariables
{
    public static bool shouldMenuTransition = false;
    public static int LevelsUnlocked { get; private set; } = 1;
    public static void IncrementLevelsUnlocked() => LevelsUnlocked++;
}
