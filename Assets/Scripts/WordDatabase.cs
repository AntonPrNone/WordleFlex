using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public static class DatabaseManager
{
    private static SQLiteConnection db;
    private static string dbPath; // ��������� ��� ���� ������


    static DatabaseManager()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "MyGame.db");
        CopyDatabaseIfNotExists(); // ����������� ���� ������ �� StreamingAssets � persistentDataPath
        db = new SQLiteConnection(dbPath);

        // �������� ������, ���� ��� �� ����������
        db.CreateTable<Word>();
        db.CreateTable<UserWord>();
        db.CreateTable<GameStatsData>();
        db.CreateTable<GameSettings>(); // ������� ������� �������� ����


        GetGameStats(); // �������� ���������� ����

        Debug.Log("������� ������� ��� ��� ����������.");
    }

    private static void CopyDatabaseIfNotExists()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "MyGame.db");

        if (!File.Exists(dbPath))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // ����������� ���� ������ ��� Android
                using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
                {
                    www.SendWebRequest();
                    while (!www.isDone) { }
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"������ �������� ���� ������: {www.error}");
                    }
                    else
                    {
                        File.WriteAllBytes(dbPath, www.downloadHandler.data);
                        Debug.Log("���� ������ ����������� � persistent data path");
                    }
                }
            }
            else
            {
                // ����������� ���� ������ ��� ������ ��������
                File.Copy(sourcePath, dbPath);
                Debug.Log("���� ������ ����������� � persistent data path");
            }
        }
        else
        {
            Debug.Log("���� ������ ��� ���������� � persistent data path");
        }
    }

    public static UserWord GetRandomUserWord()
    {
        int count = db.Table<UserWord>().Count();

        if (count == 0)
        {
            Debug.LogWarning("��� ��������� ���������������� ����.");
            return null;
        }

        var randomWord = db.Query<UserWord>("SELECT * FROM UserWord ORDER BY RANDOM() LIMIT 1").FirstOrDefault();

        if (randomWord != null)
        {
            Debug.Log($"��������� ���������������� �����: {randomWord.Value}, �����������: {randomWord.Definition}");
            return randomWord;
        }
        else
        {
            Debug.LogWarning("�� ������� �������� ��������� �����.");
            return null;
        }
    }

    public static void RemoveUserWord(string value)
    {
        var userWord = db.Table<UserWord>().FirstOrDefault(w => w.Value == value.ToUpper())
                     ?? db.Table<UserWord>().FirstOrDefault(w => w.Value == value.ToLower())
                     ?? db.Table<UserWord>().FirstOrDefault(w => w.Value == char.ToUpper(value[0]) + value.Substring(1).ToLower());

        if (userWord != null)
        {
            db.Delete(userWord);
            Debug.Log($"������� ���������������� �����: {userWord.Value}");
        }
        else
        {
            Debug.LogWarning($"����� '{value}' �� ������� � ���������������� ������ �� � ����� ��������.");
        }

        // ���������� ������� UserWord, ���� ��� �����
        int userWordsCount = db.Table<UserWord>().Count();
        if (userWordsCount == 0)
        {
            db.Execute("INSERT INTO UserWord (Value) SELECT Value FROM Word");
            Debug.Log("��� ����� ����������� �� Word � UserWord �������� ����� SQL-������.");
        }
    }

    public static void SaveGameStats(bool isWin, int score)
    {
        var stats = db.Table<GameStatsData>().FirstOrDefault();

        if (stats != null)
        {
            stats.GamesPlayed++;

            if (isWin)
            {
                stats.Wins++;
            }

            var scores = stats.Scores;
            scores.Add(score);
            stats.Scores = scores;

            db.InsertOrReplace(stats);
            Debug.Log("���������� ���� ��������� � ���������.");
        }
        else
        {
            stats = new GameStatsData
            {
                GamesPlayed = 1,
                Wins = isWin ? 1 : 0,
                Scores = new List<int> { score }
            };

            db.Insert(stats);
            Debug.Log("������� ����� ������ ����������.");
        }

        GameStats.SetStats(stats.GamesPlayed, stats.Wins, stats.Scores);
    }

    public static void GetGameStats()
    {
        var stats = db.Table<GameStatsData>().FirstOrDefault();

        if (stats != null)
        {
            GameStats.SetStats(stats.GamesPlayed, stats.Wins, stats.Scores);
            Debug.Log("���������� ���� ��������� �� ���� ������.");
        }
        else
        {
            Debug.Log("���������� ���� �� ������� � ���� ������.");
        }
    }

    public static void LoadWordsFromJsonFile(bool clearBeforeAdding)
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "russian_nouns_with_definition.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("���� � ������� �� ������.");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        JObject wordsData = JObject.Parse(jsonData);

        if (clearBeforeAdding)
        {
            db.DeleteAll<Word>(); // ������� ������� Word
            Debug.Log("������� Word �������.");
        }

        foreach (var word in wordsData.Properties())
        {
            string wordValue = word.Name.ToUpper();

            if (wordValue.Length != 5)
            {
                Debug.Log($"����� '{wordValue}' �� ��������� (����� �� 5).");
                continue;
            }

            string definition = word.Value["definition"]?.ToString();

            // �������� �� ������� �������������� ����� � ��������
            if (!string.IsNullOrEmpty(definition) && definition.Contains("��."))
            {
                Debug.Log($"����� '{wordValue}' �� ��������� (������������� �����).");
                continue;
            }

            db.Insert(new Word { Value = wordValue, Definition = definition });
            Debug.Log($"��������� �����: {wordValue} � ������������: {definition}");
        }

        Debug.Log("�������� ���� ���������.");
    }


    public static void ResetUserWordsAndStats()
    {
        // ������� ��� ���������������� �����
        db.DeleteAll<UserWord>();
        Debug.Log("��� ���������������� ����� �������.");

        // ��������� ��� ����� �� ������� Word � ������� UserWord
        db.Execute("INSERT INTO UserWord (Value, Definition) SELECT Value, Definition FROM Word");
        Debug.Log("���������������� ����� ������������� �� Word.");

        // ���������� ���������� ����
        db.DeleteAll<GameStatsData>();
        Debug.Log("���������� ���� ��������.");

        // ������� ������ ����������
        var newStats = new GameStatsData
        {
            GamesPlayed = 0,
            Wins = 0,
            Scores = new List<int>()
        };

        db.Insert(newStats);
        GameStats.SetStats(newStats.GamesPlayed, newStats.Wins, newStats.Scores);

        Debug.Log("������ ���������� ���� ������� � ���������.");
    }

    public static void SaveGameSettings(bool? saveWordLose = null, bool? saveWordWin = null, bool? animBG = null)
    {
        var settings = db.Table<GameSettings>().FirstOrDefault();

        if (settings != null)
        {
            // ��������� ������ �� ����, ������� ���� ��������
            if (saveWordLose.HasValue)
            {
                settings.SaveWordLose = saveWordLose.Value; // ��������� ���� �������
            }

            if (saveWordWin.HasValue)
            {
                settings.SaveWordWin = saveWordWin.Value; // ��������� ���� �������
            }

            settings.AnimBG = animBG; // ��������� ���� ���� animBG null

            db.InsertOrReplace(settings);
            Debug.Log("��������� ���� ���������.");
        }
        else
        {
            // ���� ��������� �� �������, ������� �����
            settings = new GameSettings
            {
                SaveWordLose = saveWordLose ?? false, // �������� �� ���������, ���� �� �������
                SaveWordWin = saveWordWin ?? true, // �������� �� ���������, ���� �� �������
                AnimBG = animBG // ��������� null, ���� �� �������
            };
            db.Insert(settings);
            Debug.Log("������� ����� ��������� ����.");
        }
    }


    public static GameSettings GetGameSettings()
    {
        var settings = db.Table<GameSettings>().FirstOrDefault();

        if (settings != null)
        {
            Debug.Log($"��������� ���� ���������: SaveWordLose = {settings.SaveWordLose}, SaveWordWin = {settings.SaveWordWin}");
            return settings;
        }
        else
        {
            Debug.LogWarning("��������� ���� �� �������. ������������ ��������� �� ���������.");
            SaveGameSettings(false, true, false);
            return new GameSettings { SaveWordLose = false, SaveWordWin = true, AnimBG = false }; // ��������� �� ���������
        }
    }
}

public class BaseWord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Value { get; set; }
    public string Definition { get; set; }
}

public class Word : BaseWord { }
public class UserWord : BaseWord { }

public class GameStatsData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int GamesPlayed { get; set; }
    public int Wins { get; set; }
    public string ScoresJson { get; set; }

    [Ignore]
    public List<int> Scores
    {
        get => JsonUtility.FromJson<ListWrapper<int>>(ScoresJson)?.List ?? new List<int>();
        set => ScoresJson = JsonUtility.ToJson(new ListWrapper<int> { List = value });
    }
}

public class GameSettings
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public bool SaveWordLose { get; set; }
    public bool SaveWordWin { get; set; }

    public bool? AnimBG { get; set; }
}

[Serializable]
public class ListWrapper<T>
{
    public List<T> List;
}
