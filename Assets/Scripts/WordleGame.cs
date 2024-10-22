using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WordleGame : MonoBehaviour
{
    public ErrorDisplay errorDisplay;
    public GameTimer gameTimer;
    public int rows = 6;
    public int columns = 5;
    public UserWord targetUserWordResult;
    public GameObject cellPrefab;
    public List<TMP_Text> gridTexts = new();

    // Поля для HUD
    public GameObject hudContainer;
    public TMP_Text resultText;
    public TMP_Text wordText;
    public TMP_Text definition;
    public TMP_Text timeText;
    public TMP_Text scoreText;

    internal int currentRow = 0;
    internal int currentColumn = 0;
    internal GameObject currentActiveCell;
    internal bool gameActive = true;

    public float animationDuration = 0.25f;

    private const int MaxAttempts = 6;
    private const int PointsPerAttempt = 100;
    private const int MaxTimeScore = 500;

    public List<string> usedWords = new();
    public bool saveWordLose = true;
    public bool saveWordWin = false;

    private KeyboardManager[] allKeyboardManagers;

    void Start()
    {
        //DatabaseManager.LoadWordsFromJsonFile(true);
        //DatabaseManager.ResetUserWordsAndStats();

        targetUserWordResult = DatabaseManager.GetRandomUserWord();
        targetUserWordResult.Value = targetUserWordResult.Value.ToUpper();

        GenerateGrid();
        gameTimer.StartGame();

        hudContainer.SetActive(false);

        allKeyboardManagers = FindObjectsOfType<KeyboardManager>();

        saveWordWin = DatabaseManager.GetGameSettings().SaveWordWin;
        saveWordLose = DatabaseManager.GetGameSettings().SaveWordLose;
    }

    void GenerateGrid()
    {
        for (int i = 0; i < rows * columns; i++)
        {
            GameObject newCell = Instantiate(cellPrefab, transform);
            TMP_Text text = newCell.GetComponentInChildren<TMP_Text>();
            text.text = "";
            newCell.AddComponent<CellClickHandler>().Setup(this, i);
            gridTexts.Add(text);
        }
    }

    void Update()
    {
        if (!gameActive) return;

        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && IsCurrentRowFilled())
        {
            ConfirmWord();
            return;
        }

        if (Input.anyKeyDown && currentColumn < columns)
        {
            foreach (char c in Input.inputString)
            {
                if (char.IsLetter(c) && currentColumn < columns)
                {
                    InputLetter(c.ToString().ToUpper());
                    break;
                }
                else if (c == '\b')
                {
                    RemoveCurrentLetter();
                }
            }
        }
    }

    public void InputLetter(string letter)
    {
        gridTexts[currentRow * columns + currentColumn].text = letter;

        if (currentColumn < columns - 1)
        {
            currentColumn++;
            ActivateCell(gridTexts[currentRow * columns + currentColumn].transform.parent.gameObject);
        }
    }

    public void RemoveCurrentLetter()
    {
        gridTexts[currentRow * columns + currentColumn].text = "";

        if (currentColumn > 0)
        {
            currentColumn--;
            ActivateCell(gridTexts[currentRow * columns + currentColumn].transform.parent.gameObject);
        }
    }

    public void ConfirmWord()
    {
        string inputWord = GetWordFromRow(currentRow);

        // Проверка, использовалось ли слово ранее
        if (usedWords.Contains(inputWord))
        {
            errorDisplay.ShowErrorPanel("Слово уже было использовано!");
            return; // Останавливаем выполнение, если слово уже было
        }

        // Добавляем текущее слово в список использованных
        usedWords.Add(inputWord.ToUpper());

        CheckWord(inputWord);

        if (inputWord.ToUpper() == targetUserWordResult.Value)
        {
            gameTimer.StopGame();
            int secondTotal = gameTimer.ElapsedSeconds;
            Debug.Log("Слово угадано!");
            DatabaseManager.SaveGameStats(true, CalculateScore(currentRow + 1, secondTotal));
            if (!saveWordWin) DatabaseManager.RemoveUserWord(targetUserWordResult.Value);
            gameActive = false;

            DisplayHUD(true);
        }
        else
        {
            if (currentRow < rows - 1)
            {
                currentRow++;
                currentColumn = 0;
                ActivateCell(gridTexts[currentRow * columns + currentColumn].transform.parent.gameObject);
            }
            else
            {
                gameTimer.StopGame();
                Debug.Log("Вы проиграли!");
                DatabaseManager.SaveGameStats(false, 0);
                if (!saveWordLose) DatabaseManager.RemoveUserWord(targetUserWordResult.Value);
                gameActive = false;

                DisplayHUD(false);
            }
        }

        foreach (KeyboardManager obj in allKeyboardManagers) obj.UpdateLetterColors(inputWord);
    }

    public string GetWordFromRow(int row)
    {
        string word = "";
        for (int i = row * columns; i < (row + 1) * columns; i++)
        {
            word += gridTexts[i].text;
        }
        return word;
    }

    public void CheckWord(string inputWord)
    {
        inputWord = inputWord.ToUpper();

        for (int i = 0; i < columns; i++)
        {
            int cellIndex = currentRow * columns + i;
            TMP_Text cellText = gridTexts[cellIndex];

            if (inputWord[i] == targetUserWordResult.Value[i])
            {
                AnimateCellFlipX(cellText, Color.green);
            }
            else if (targetUserWordResult.Value.Contains(inputWord[i].ToString()))
            {
                AnimateCellFlipY(cellText, Color.yellow);
            }
            else
            {
                cellText.color = Color.gray;
            }
        }
    }

    private void AnimateCellFlipX(TMP_Text cellText, Color targetColor)
    {
        LeanTween.rotateX(cellText.gameObject, 90f, 0.25f).setOnComplete(() =>
        {
            cellText.color = targetColor;
            LeanTween.rotateX(cellText.gameObject, 0f, 0.25f);
        });
    }

    private void AnimateCellFlipY(TMP_Text cellText, Color targetColor)
    {
        LeanTween.rotateY(cellText.gameObject, 90f, 0.25f).setOnComplete(() =>
        {
            cellText.color = targetColor;
            LeanTween.rotateY(cellText.gameObject, 0f, 0.25f);
        });
    }

    public void OnCellClicked(int cellIndex)
    {
        int clickedRow = cellIndex / columns;

        if (clickedRow == currentRow)
        {
            currentColumn = cellIndex % columns;
            ActivateCell(gridTexts[cellIndex].transform.parent.gameObject);
        }
    }

    public void ActivateCell(GameObject cell)
    {
        if (currentActiveCell == cell) return;

        Image cellImage = cell.GetComponent<Image>();
        Outline outline = cell.GetComponent<Outline>();

        if (outline != null && cellImage != null)
        {
            DeactivateCurrentCell();

            Image prefabImage = cellPrefab.GetComponent<Image>();
            Outline prefabOutline = cellPrefab.GetComponent<Outline>();

            Color originalOutlineColor = prefabOutline.effectColor;
            Color originalImageColor = prefabImage.color;
            Vector2 originalOutlineThickness = prefabOutline.effectDistance;

            LeanTween.value(cell, originalOutlineColor, Color.white, animationDuration)
                .setOnUpdate(val => outline.effectColor = val);
            LeanTween.value(cell, originalOutlineThickness.x, originalOutlineThickness.x + 2, animationDuration)
                .setOnUpdate(val => outline.effectDistance = new Vector2(val, val));
            LeanTween.value(cell, originalImageColor, Color.white, animationDuration)
                .setOnUpdate(val => cellImage.color = val);
        }

        currentActiveCell = cell;
    }

    public void DeactivateCurrentCell()
    {
        if (currentActiveCell != null)
        {
            Image cellImage = currentActiveCell.GetComponent<Image>();
            Outline outline = currentActiveCell.GetComponent<Outline>();

            if (outline != null && cellImage != null)
            {
                Image prefabImage = cellPrefab.GetComponent<Image>();
                Outline prefabOutline = cellPrefab.GetComponent<Outline>();

                Color targetOutlineColor = prefabOutline.effectColor;
                Vector2 targetOutlineThickness = prefabOutline.effectDistance;
                Color targetImageColor = prefabImage.color;

                LeanTween.value(currentActiveCell, Color.white, targetOutlineColor, animationDuration)
                    .setOnUpdate(val => outline.effectColor = val);
                LeanTween.value(currentActiveCell, targetOutlineThickness.x + 2, targetOutlineThickness.x, animationDuration)
                    .setOnUpdate(val => outline.effectDistance = new Vector2(val, val));
                LeanTween.value(currentActiveCell, Color.white, targetImageColor, animationDuration)
                    .setOnUpdate(val => cellImage.color = val);
            }

            currentActiveCell = null;
        }
    }

    public bool IsCurrentRowFilled()
    {
        for (int i = 0; i < columns; i++)
        {
            if (string.IsNullOrEmpty(gridTexts[currentRow * columns + i].text))
            {
                errorDisplay.ShowErrorPanel("Заполните слово полностью!");
                return false;
            }
        }
        return true;
    }

    private void DisplayHUD(bool wordGuessed)
    {
        int secondTotal = gameTimer.ElapsedSeconds;

        resultText.text = wordGuessed ? "Слово отгадано!" : "Слово не отгадано...";
        resultText.color = wordGuessed ? new Color32(69, 255, 77, 255) : new Color32(255, 95, 95, 255);

        wordText.text = targetUserWordResult.Value;
        definition.text = targetUserWordResult.Definition;
        timeText.text = gameTimer.timerText.text;
        scoreText.text = wordGuessed ? CalculateScore(currentRow + 1, secondTotal).ToString() : "0";

        hudContainer.transform.localScale = Vector3.zero;
        hudContainer.SetActive(true);
        LeanTween.scale(hudContainer, Vector3.one, 0.25f).setEase(LeanTweenType.easeOutBack);
    }

    public void OnPlayAgainButtonClick()
    {
        LeanTween.scale(hudContainer, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            hudContainer.SetActive(false);
        });

        StartNewGame();
    }

    public void StartNewGame()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        targetUserWordResult = DatabaseManager.GetRandomUserWord();
        targetUserWordResult.Value = targetUserWordResult.Value.ToUpper(); currentRow = 0;

        currentColumn = 0;
        gameActive = true;
        currentActiveCell = null;

        gridTexts.Clear();
        GenerateGrid();
        KeyboardManager.ResetKeyboardColors();

        gameTimer.ResetGame();
        gameTimer.StartGame();

        usedWords.Clear();
    }


    public static int CalculateScore(int attempts, int timeSpent)
    {
        attempts = Mathf.Clamp(attempts, 0, MaxAttempts);

        int scoreForAttempts = attempts switch
        {
            1 => PointsPerAttempt * 5,
            2 => PointsPerAttempt * 4,
            3 => PointsPerAttempt * 3,
            4 => PointsPerAttempt * 2,
            5 => PointsPerAttempt * 1,
            _ => 0,
        };

        int scoreForTime = MaxTimeScore - Mathf.Clamp(timeSpent, 0, MaxTimeScore);
        int totalScore = scoreForAttempts + scoreForTime;
        return totalScore;
    }

    public void ToggleSwithSaveWordLose(bool saveWordLoseValue) 
    {
        saveWordLose = !saveWordLose;
        DatabaseManager.SaveGameSettings(saveWordLose: saveWordLose, saveWordWin: saveWordWin);
    }

    public void ToggleSwithSaveWordLWin(bool saveWordWinValue) 
    {
        saveWordWin = !saveWordWin;
        DatabaseManager.SaveGameSettings(saveWordLose: saveWordLose, saveWordWin: saveWordWin);
    }
}
