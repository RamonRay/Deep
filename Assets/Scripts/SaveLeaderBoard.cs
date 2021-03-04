using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveLeaderBoard{

    public static void Save(LeaderBoard leaderBoardData)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/LeaderBoard.data";
        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, leaderBoardData);
        stream.Close();
    }

    public static LeaderBoard Load()
    {
        string path = Application.persistentDataPath + "/LeaderBoard.data";
        Debug.Log(path);
        if(File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            LeaderBoard leaderBoardData = formatter.Deserialize(stream) as LeaderBoard;
            stream.Close();
            return leaderBoardData;
        }
        else
        {
            Debug.LogError("Save file not found");
            return new LeaderBoard();
        }
    }
}
