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
    private static string dbPath; // Объявляем как поле класса


    static DatabaseManager()
    {
        dbPath = Path.Combine(Application.persistentDataPath, "MyGame.db");
        CopyDatabaseIfNotExists(); // Копирование базы данных из StreamingAssets в persistentDataPath
        db = new SQLiteConnection(dbPath);

        // Создание таблиц, если они не существуют
        db.CreateTable<Word>();
        db.CreateTable<UserWord>();
        db.CreateTable<GameStatsData>();
        db.CreateTable<GameSettings>(); // Создаем таблицу настроек игры


        GetGameStats(); // Загрузка статистики игры

        Debug.Log("Таблицы созданы или уже существуют.");
    }

    private static void CopyDatabaseIfNotExists()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "MyGame.db");

        if (!File.Exists(dbPath))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                // Копирование базы данных для Android
                using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
                {
                    www.SendWebRequest();
                    while (!www.isDone) { }
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Ошибка загрузки базы данных: {www.error}");
                    }
                    else
                    {
                        File.WriteAllBytes(dbPath, www.downloadHandler.data);
                        Debug.Log("База данных скопирована в persistent data path");
                    }
                }
            }
            else
            {
                // Копирование базы данных для других платформ
                File.Copy(sourcePath, dbPath);
                Debug.Log("База данных скопирована в persistent data path");
            }
        }
        else
        {
            Debug.Log("База данных уже существует в persistent data path");
        }
    }

    public static UserWord GetRandomUserWord()
    {
        int count = db.Table<UserWord>().Count();

        if (count == 0)
        {
            Debug.LogWarning("Нет доступных пользовательских слов.");
            return null;
        }

        var randomWord = db.Query<UserWord>("SELECT * FROM UserWord ORDER BY RANDOM() LIMIT 1").FirstOrDefault();

        if (randomWord != null)
        {
            Debug.Log($"Случайное пользовательское слово: {randomWord.Value}, Определение: {randomWord.Definition}");
            return randomWord;
        }
        else
        {
            Debug.LogWarning("Не удалось получить случайное слово.");
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
            Debug.Log($"Удалено пользовательское слово: {userWord.Value}");
        }
        else
        {
            Debug.LogWarning($"Слово '{value}' не найдено в пользовательских словах ни в одном регистре.");
        }

        // Заполнение таблицы UserWord, если она пуста
        int userWordsCount = db.Table<UserWord>().Count();
        if (userWordsCount == 0)
        {
            db.Execute("INSERT INTO UserWord (Value) SELECT Value FROM Word");
            Debug.Log("Все слова скопированы из Word в UserWord напрямую через SQL-запрос.");
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
            Debug.Log("Статистика игры обновлена и сохранена.");
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
            Debug.Log("Создана новая запись статистики.");
        }

        GameStats.SetStats(stats.GamesPlayed, stats.Wins, stats.Scores);
    }

    public static void GetGameStats()
    {
        var stats = db.Table<GameStatsData>().FirstOrDefault();

        if (stats != null)
        {
            GameStats.SetStats(stats.GamesPlayed, stats.Wins, stats.Scores);
            Debug.Log("Статистика игры загружена из базы данных.");
        }
        else
        {
            Debug.Log("Статистика игры не найдена в базе данных.");
        }
    }

    public static void LoadWordsFromJsonFile(bool clearBeforeAdding)
    {
        string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "russian_nouns_with_definition.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError("Файл с данными не найден.");
            return;
        }

        string jsonData = File.ReadAllText(filePath);
        JObject wordsData = JObject.Parse(jsonData);

        if (clearBeforeAdding)
        {
            db.DeleteAll<Word>(); // Очищаем таблицу Word
            Debug.Log("Таблица Word очищена.");
        }

        foreach (var word in wordsData.Properties())
        {
            string wordValue = word.Name.ToUpper();

            if (wordValue.Length != 5)
            {
                Debug.Log($"Слово '{wordValue}' не добавлено (длина не 5).");
                continue;
            }

            string definition = word.Value["definition"]?.ToString();

            // Проверка на наличие множественного числа в описании
            if (!string.IsNullOrEmpty(definition) && definition.Contains("мн."))
            {
                Debug.Log($"Слово '{wordValue}' не добавлено (множественное число).");
                continue;
            }

            db.Insert(new Word { Value = wordValue, Definition = definition });
            Debug.Log($"Добавлено слово: {wordValue} с определением: {definition}");
        }

        Debug.Log("Загрузка слов завершена.");
    }


    public static void ResetUserWordsAndStats()
    {
        // Удаляем все пользовательские слова
        db.DeleteAll<UserWord>();
        Debug.Log("Все пользовательские слова удалены.");

        // Вставляем все слова из таблицы Word в таблицу UserWord
        db.Execute("INSERT INTO UserWord (Value, Definition) SELECT Value, Definition FROM Word");
        Debug.Log("Пользовательские слова восстановлены из Word.");

        // Сбрасываем статистику игры
        db.DeleteAll<GameStatsData>();
        Debug.Log("Статистика игры сброшена.");

        // Создаем пустую статистику
        var newStats = new GameStatsData
        {
            GamesPlayed = 0,
            Wins = 0,
            Scores = new List<int>()
        };

        db.Insert(newStats);
        GameStats.SetStats(newStats.GamesPlayed, newStats.Wins, newStats.Scores);

        Debug.Log("Пустая статистика игры создана и сохранена.");
    }

    public static void SaveGameSettings(bool? saveWordLose = null, bool? saveWordWin = null, bool? animBG = null)
    {
        var settings = db.Table<GameSettings>().FirstOrDefault();

        if (settings != null)
        {
            // Обновляем только те поля, которые были переданы
            if (saveWordLose.HasValue)
            {
                settings.SaveWordLose = saveWordLose.Value; // Обновляем если передан
            }

            if (saveWordWin.HasValue)
            {
                settings.SaveWordWin = saveWordWin.Value; // Обновляем если передан
            }

            settings.AnimBG = animBG; // Обновляем даже если animBG null

            db.InsertOrReplace(settings);
            Debug.Log("Настройки игры обновлены.");
        }
        else
        {
            // Если настройки не найдены, создаем новые
            settings = new GameSettings
            {
                SaveWordLose = saveWordLose ?? false, // Значение по умолчанию, если не передан
                SaveWordWin = saveWordWin ?? true, // Значение по умолчанию, если не передан
                AnimBG = animBG // Оставляем null, если не передан
            };
            db.Insert(settings);
            Debug.Log("Созданы новые настройки игры.");
        }
    }


    public static GameSettings GetGameSettings()
    {
        var settings = db.Table<GameSettings>().FirstOrDefault();

        if (settings != null)
        {
            Debug.Log($"Настройки игры загружены: SaveWordLose = {settings.SaveWordLose}, SaveWordWin = {settings.SaveWordWin}");
            return settings;
        }
        else
        {
            Debug.LogWarning("Настройки игры не найдены. Используются настройки по умолчанию.");
            SaveGameSettings(false, true, false);
            return new GameSettings { SaveWordLose = false, SaveWordWin = true, AnimBG = false }; // Настройки по умолчанию
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
