using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    public GameObject errorPanel;  // Панель с сообщением об ошибке
    private RectTransform errorPanelRect;  // RectTransform панели
    private CanvasGroup canvasGroup;  // CanvasGroup для управления прозрачностью

    private bool isAnimating = false;  // Флаг для проверки, идёт ли анимация
    private float panelWidth;  // Для сохранения ширины панели
    private Vector2 initialPosition;  // Для сохранения исходной позиции панели

    private void Start()
    {
        // Получаем RectTransform панели и её ширину
        errorPanelRect = errorPanel.GetComponent<RectTransform>();
        canvasGroup = errorPanel.GetComponent<CanvasGroup>();
        panelWidth = errorPanelRect.rect.width;

        // Сохраняем исходную позицию панели (включая Y)
        initialPosition = errorPanelRect.anchoredPosition;

        // Изначально панель скрыта и находится за пределами видимости (на ширину панели)
        canvasGroup.alpha = 0;
        errorPanelRect.anchoredPosition = new Vector2(panelWidth, initialPosition.y);
    }

    public void ShowErrorPanel(string message)
    {
        if (isAnimating) return;  // Если анимация уже идёт, ничего не делаем

        isAnimating = true;  // Начинаем анимацию
        errorPanel.SetActive(true);  // Включаем панель

        // Обновляем текст ошибки
        errorPanel.GetComponentInChildren<TMP_Text>().text = message;

        // Анимация появления (выезд справа и плавное проявление)
        LeanTween.moveX(errorPanelRect, initialPosition.x, 0.5f).setEase(LeanTweenType.easeOutExpo);  // Возвращаем панель на исходную позицию
        LeanTween.alphaCanvas(canvasGroup, 1, 0.5f);  // Плавно делаем её видимой

        // Показать на 2 секунды, затем затухание и отключение
        LeanTween.delayedCall(2f, () =>
        {
            LeanTween.moveX(errorPanelRect, panelWidth, 0.5f).setEase(LeanTweenType.easeInExpo);  // Уезжает за пределы
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                errorPanel.SetActive(false);  // Отключаем панель
                isAnimating = false;  // Завершаем анимацию
            });
        });
    }

    public void ShowErrorPanel()
    {
        if (isAnimating) return;  // Если анимация уже идёт, ничего не делаем

        isAnimating = true;  // Начинаем анимацию
        errorPanel.SetActive(true);  // Включаем панель

        // Анимация появления (выезд справа и плавное проявление)
        LeanTween.moveX(errorPanelRect, initialPosition.x, 0.5f).setEase(LeanTweenType.easeOutExpo);  // Возвращаем панель на исходную позицию
        LeanTween.alphaCanvas(canvasGroup, 1, 0.5f);  // Плавно делаем её видимой

        // Показать на 2 секунды, затем затухание и отключение
        LeanTween.delayedCall(2f, () =>
        {
            LeanTween.moveX(errorPanelRect, panelWidth, 0.5f).setEase(LeanTweenType.easeInExpo);  // Уезжает за пределы
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                errorPanel.SetActive(false);  // Отключаем панель
                isAnimating = false;  // Завершаем анимацию
            });
        });
    }
}
