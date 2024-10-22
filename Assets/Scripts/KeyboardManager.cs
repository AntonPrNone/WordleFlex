using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class KeyboardManager : MonoBehaviour
{
    private WordleGame wordleGame;
    private Outline outline;
    private bool isAnimating = false;

    private Color correctLetterColor = new Color32(0, 188, 0, 255);
    private Color wrongLetterColor = new Color32(90, 90, 90, 255);
    private static Color defaultLetterColor = new Color32(34, 34, 34, 255);

    private static List<GameObject> letterKeys;

    private TouchScreenKeyboard androidKeyboard;


    void Start()
    {
        wordleGame = FindObjectOfType<WordleGame>();
        outline = GetComponent<Outline>();

        letterKeys = new List<GameObject>(GameObject.FindGameObjectsWithTag("CellKey"));
    }

    void Update()
    {
        // ќбработка Android-клавиатуры
        if (androidKeyboard != null && androidKeyboard.status == TouchScreenKeyboard.Status.Visible)
        {
            string input = androidKeyboard.text;

            if (!string.IsNullOrEmpty(input))
            {
                foreach (char c in input)
                {
                    // Ёмулируем нажатие клавиши
                    ProcessInput(c.ToString());
                }

                androidKeyboard.text = ""; // ќчищаем, чтобы не было повторного ввода
            }
        }
    }

    public void UpdateLetterColors(string inputWord)
    {
        foreach (var key in letterKeys)
        {
            TMP_Text letterText = key.GetComponentInChildren<TMP_Text>();
            if (letterText != null)
            {
                string letter = letterText.text.ToUpper();

                if (inputWord.Contains(letter))
                {
                    if (wordleGame.targetUserWordResult.Value.Contains(letter))
                    {
                        key.GetComponent<Image>().color = correctLetterColor;
                    }
                    else
                    {
                        key.GetComponent<Image>().color = wrongLetterColor;
                    }
                }
            }
        }
    }

    public void OnLetterButtonClick()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (wordleGame != null && wordleGame.gameActive)
        {
            TMP_Text letterText = GetComponentInChildren<TMP_Text>();
            if (letterText != null)
            {
                string letter = letterText.text.ToUpper();
                wordleGame.InputLetter(letter);
            }

            if (!isAnimating)
            {
                AnimateButtonClick();
            }
        }
    }

    public void RemoveLetter()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (wordleGame != null && wordleGame.gameActive)
        {
            wordleGame.RemoveCurrentLetter();

            if (outline != null)
            {
                if (!isAnimating)
                {
                    AnimateRemoveLetter();
                }
            }
        }
    }

    public void OnEnterButtonClick()
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (wordleGame != null && wordleGame.IsCurrentRowFilled() && wordleGame.gameActive)
        {
            if (outline != null)
            {
                if (!isAnimating)
                {
                    AnimateEnterButton();
                    UpdateLetterColors(wordleGame.GetWordFromRow(wordleGame.currentRow));
                }
            }
            wordleGame.ConfirmWord();
        }
    }

    private void AnimateButtonClick()
    {
        isAnimating = true;

        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 0.8f;

        LeanTween.scale(gameObject, targetScale, 0.125f)
            .setOnComplete(() => LeanTween.scale(gameObject, originalScale, 0.125f)
            .setOnComplete(() => isAnimating = false));
    }

    private void AnimateRemoveLetter()
    {
        isAnimating = true;

        Color originalColor = outline.effectColor;
        Color redColor = new Color32(255, 95, 95, 255);
        float animationDuration = 0.25f;

        LeanTween.value(gameObject, originalColor, redColor, animationDuration)
            .setOnUpdate(val => outline.effectColor = val)
            .setOnComplete(() =>
            {
                LeanTween.value(gameObject, redColor, originalColor, animationDuration)
                    .setOnUpdate(val => outline.effectColor = val)
                    .setOnComplete(() => isAnimating = false);
            });
    }

    private void AnimateEnterButton()
    {
        isAnimating = true;

        Color originalColor = outline.effectColor;
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float animationDuration = 0.25f;

        LeanTween.value(gameObject, originalColor, correctLetterColor, animationDuration)
            .setOnUpdate(val => outline.effectColor = val)
            .setOnComplete(() =>
            {
                LeanTween.value(gameObject, correctLetterColor, originalColor, animationDuration)
                    .setOnUpdate(val => outline.effectColor = val)
                    .setOnComplete(() => isAnimating = false);
            });

        LeanTween.scale(gameObject, targetScale, animationDuration)
            .setOnComplete(() => LeanTween.scale(gameObject, originalScale, animationDuration)
            .setOnComplete(() => isAnimating = false));
    }

    public static void ResetKeyboardColors()
    {
        foreach (var key in letterKeys)
        {
            key.GetComponent<Image>().color = defaultLetterColor;
        }
    }

    public void ToggleKeyboard()
    {
        if (androidKeyboard == null || !TouchScreenKeyboard.visible)
        {
            androidKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false);
        }
        else
        {
            androidKeyboard.active = false;
            androidKeyboard = null;
        }
    }

    private void ProcessInput(string keyPressed)
    {
        // »спользуем уже существующий метод дл€ обработки нажатий
        wordleGame.InputLetter(keyPressed.ToUpper());
    }
}
