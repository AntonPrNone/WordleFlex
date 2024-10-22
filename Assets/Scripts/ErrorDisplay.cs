using TMPro;
using UnityEngine;

public class ErrorDisplay : MonoBehaviour
{
    public GameObject errorPanel;  // ������ � ���������� �� ������
    private RectTransform errorPanelRect;  // RectTransform ������
    private CanvasGroup canvasGroup;  // CanvasGroup ��� ���������� �������������

    private bool isAnimating = false;  // ���� ��� ��������, ��� �� ��������
    private float panelWidth;  // ��� ���������� ������ ������
    private Vector2 initialPosition;  // ��� ���������� �������� ������� ������

    private void Start()
    {
        // �������� RectTransform ������ � � ������
        errorPanelRect = errorPanel.GetComponent<RectTransform>();
        canvasGroup = errorPanel.GetComponent<CanvasGroup>();
        panelWidth = errorPanelRect.rect.width;

        // ��������� �������� ������� ������ (������� Y)
        initialPosition = errorPanelRect.anchoredPosition;

        // ���������� ������ ������ � ��������� �� ��������� ��������� (�� ������ ������)
        canvasGroup.alpha = 0;
        errorPanelRect.anchoredPosition = new Vector2(panelWidth, initialPosition.y);
    }

    public void ShowErrorPanel(string message)
    {
        if (isAnimating) return;  // ���� �������� ��� ���, ������ �� ������

        isAnimating = true;  // �������� ��������
        errorPanel.SetActive(true);  // �������� ������

        // ��������� ����� ������
        errorPanel.GetComponentInChildren<TMP_Text>().text = message;

        // �������� ��������� (����� ������ � ������� ����������)
        LeanTween.moveX(errorPanelRect, initialPosition.x, 0.5f).setEase(LeanTweenType.easeOutExpo);  // ���������� ������ �� �������� �������
        LeanTween.alphaCanvas(canvasGroup, 1, 0.5f);  // ������ ������ � �������

        // �������� �� 2 �������, ����� ��������� � ����������
        LeanTween.delayedCall(2f, () =>
        {
            LeanTween.moveX(errorPanelRect, panelWidth, 0.5f).setEase(LeanTweenType.easeInExpo);  // ������� �� �������
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                errorPanel.SetActive(false);  // ��������� ������
                isAnimating = false;  // ��������� ��������
            });
        });
    }

    public void ShowErrorPanel()
    {
        if (isAnimating) return;  // ���� �������� ��� ���, ������ �� ������

        isAnimating = true;  // �������� ��������
        errorPanel.SetActive(true);  // �������� ������

        // �������� ��������� (����� ������ � ������� ����������)
        LeanTween.moveX(errorPanelRect, initialPosition.x, 0.5f).setEase(LeanTweenType.easeOutExpo);  // ���������� ������ �� �������� �������
        LeanTween.alphaCanvas(canvasGroup, 1, 0.5f);  // ������ ������ � �������

        // �������� �� 2 �������, ����� ��������� � ����������
        LeanTween.delayedCall(2f, () =>
        {
            LeanTween.moveX(errorPanelRect, panelWidth, 0.5f).setEase(LeanTweenType.easeInExpo);  // ������� �� �������
            LeanTween.alphaCanvas(canvasGroup, 0, 0.5f).setOnComplete(() =>
            {
                errorPanel.SetActive(false);  // ��������� ������
                isAnimating = false;  // ��������� ��������
            });
        });
    }
}
