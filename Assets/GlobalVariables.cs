using UnityEngine;

public static class GlobalVariables
{
    public static bool shouldMenuTransition = false;
    public static int LevelsUnlocked { get; private set; } = 1;
    public static void IncrementLevelsUnlocked()
    {
        //Sobe o nível da variável
        LevelsUnlocked++;

        //Salva o valor da variável
        SavingSystem.SaveLevel(LevelsUnlocked);
    }

    public static void LoadLevelsUnlocked()
    {
        LevelsUnlocked = SavingSystem.LoadLevel();
    }
}
