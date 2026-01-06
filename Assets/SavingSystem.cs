using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public class SavingSystem
{
    public static void SaveLevel(int levelNum)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/level.bin";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, levelNum);
        stream.Close();
    }

    public static int LoadLevel()
    {
        string path = Application.persistentDataPath + "/level.bin";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            int levelNum = (int)formatter.Deserialize(stream);
            stream.Close();

            return levelNum;
        }
        else
        {
            SaveLevel(1);
            return 1;
        }
    }
}
