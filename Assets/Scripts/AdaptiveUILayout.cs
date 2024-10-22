using UnityEngine;
using UnityEngine.UI;

public class AdaptiveUILayout : MonoBehaviour
{
    public RectTransform grid; // Ссылка на RectTransform сетки
    public RectTransform logo; // Ссылка на RectTransform логотипа
    public RectTransform keyboardUI; // Ссылка на RectTransform UI-клавиатуры
    private TouchScreenKeyboard mobileKeyboard; // Ссылка на экранную клавиатуру

    private Vector2 originalAnchorMin; // Оригинальный якорь (min)
    private Vector2 originalAnchorMax; // Оригинальный якорь (max)
    private Vector2 originalPivot; // Оригинальная точка поворота
    private Vector2 originalOffsetMin; // Оригинальный нижний отступ
    private Vector2 originalOffsetMax; // Оригинальный верхний отступ
    private Vector2 originalKeyboardPosition; // Оригинальная позиция UI-клавиатуры

    private bool isMobileLayoutApplied = false;

    void Start()
    {
        // Сохраняем оригинальные параметры для восстановления
        originalAnchorMin = grid.anchorMin;
        originalAnchorMax = grid.anchorMax;
        originalPivot = grid.pivot;
        originalOffsetMin = grid.offsetMin;
        originalOffsetMax = grid.offsetMax;
        originalKeyboardPosition = keyboardUI.anchoredPosition; // Сохраняем оригинальную позицию UI-клавиатуры
    }

    void Update()
    {
        // Проверяем, если высота больше ширины (мобильное соотношение сторон)
        if (Screen.height > Screen.width)
        {
            ApplyMobileLayout();
            RaiseUIKeyboard(); // Поднять UI-клавиатуру
        }
        else
        {
            RestoreOriginalLayout();
            RestoreUIKeyboardPosition(); // Восстановить оригинальную позицию UI-клавиатуры
        }

        // Проверка соотношения сторон экрана и изменение масштаба логотипа
        CheckAspectRatio();
    }

    void ApplyMobileLayout()
    {
        if (!isMobileLayoutApplied)
        {
            // Устанавливаем якорь по центру
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);

            // Убираем отступы
            grid.offsetMin = Vector2.zero;
            grid.offsetMax = Vector2.zero;

            Debug.Log("Применен адаптивный мобильный интерфейс.");
            isMobileLayoutApplied = true;
        }
    }

    void RestoreOriginalLayout()
    {
        if (isMobileLayoutApplied)
        {
            // Восстанавливаем оригинальные якоря, pivot и отступы
            grid.anchorMin = originalAnchorMin;
            grid.anchorMax = originalAnchorMax;
            grid.pivot = originalPivot;

            grid.offsetMin = originalOffsetMin;
            grid.offsetMax = originalOffsetMax;

            Debug.Log("Восстановлен оригинальный интерфейс.");
            isMobileLayoutApplied = false;
        }
    }

    void CheckAspectRatio()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        // Проверяем, если ширина больше высоты в диапазоне 1.25–1 раз
        if (aspectRatio >= 1.0f && aspectRatio <= 1.25f)
        {
            // Уменьшаем масштаб логотипа до 0.4
            logo.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Debug.Log("Изменен масштаб логотипа до 0.4.");
        }
        else
        {
            // Восстанавливаем оригинальный масштаб, если соотношение другое
            logo.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    // Метод для управления клавиатурой: открытие или закрытие
    public void ToggleKeyboard()
    {
        // Проверяем, открыта ли клавиатура
        if (mobileKeyboard == null || !mobileKeyboard.active)
        {
            // Если клавиатура закрыта — открываем
            mobileKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            Debug.Log("Экранная клавиатура открыта.");
        }
        else
        {
            // Если клавиатура открыта — закрываем
            mobileKeyboard.active = false;
            Debug.Log("Экранная клавиатура скрыта.");
        }
    }

    void RaiseUIKeyboard()
    {
        // Поднимаем UI-клавиатуру на 50 пикселей по оси Y
        keyboardUI.anchoredPosition = new Vector2(originalKeyboardPosition.x, originalKeyboardPosition.y + 50);
        Debug.Log("UI-клавиатура поднята на 50 пикселей.");
    }

    void RestoreUIKeyboardPosition()
    {
        // Восстанавливаем оригинальную позицию UI-клавиатуры
        keyboardUI.anchoredPosition = originalKeyboardPosition;
        Debug.Log("Оригинальная позиция UI-клавиатуры восстановлена.");
    }
}
