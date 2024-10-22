using UnityEngine;
using UnityEngine.UI;

public class AdaptiveUILayout : MonoBehaviour
{
    public RectTransform grid; // ������ �� RectTransform �����
    public RectTransform logo; // ������ �� RectTransform ��������
    public RectTransform keyboardUI; // ������ �� RectTransform UI-����������
    private TouchScreenKeyboard mobileKeyboard; // ������ �� �������� ����������

    private Vector2 originalAnchorMin; // ������������ ����� (min)
    private Vector2 originalAnchorMax; // ������������ ����� (max)
    private Vector2 originalPivot; // ������������ ����� ��������
    private Vector2 originalOffsetMin; // ������������ ������ ������
    private Vector2 originalOffsetMax; // ������������ ������� ������
    private Vector2 originalKeyboardPosition; // ������������ ������� UI-����������

    private bool isMobileLayoutApplied = false;

    void Start()
    {
        // ��������� ������������ ��������� ��� ��������������
        originalAnchorMin = grid.anchorMin;
        originalAnchorMax = grid.anchorMax;
        originalPivot = grid.pivot;
        originalOffsetMin = grid.offsetMin;
        originalOffsetMax = grid.offsetMax;
        originalKeyboardPosition = keyboardUI.anchoredPosition; // ��������� ������������ ������� UI-����������
    }

    void Update()
    {
        // ���������, ���� ������ ������ ������ (��������� ����������� ������)
        if (Screen.height > Screen.width)
        {
            ApplyMobileLayout();
            RaiseUIKeyboard(); // ������� UI-����������
        }
        else
        {
            RestoreOriginalLayout();
            RestoreUIKeyboardPosition(); // ������������ ������������ ������� UI-����������
        }

        // �������� ����������� ������ ������ � ��������� �������� ��������
        CheckAspectRatio();
    }

    void ApplyMobileLayout()
    {
        if (!isMobileLayoutApplied)
        {
            // ������������� ����� �� ������
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);

            // ������� �������
            grid.offsetMin = Vector2.zero;
            grid.offsetMax = Vector2.zero;

            Debug.Log("�������� ���������� ��������� ���������.");
            isMobileLayoutApplied = true;
        }
    }

    void RestoreOriginalLayout()
    {
        if (isMobileLayoutApplied)
        {
            // ��������������� ������������ �����, pivot � �������
            grid.anchorMin = originalAnchorMin;
            grid.anchorMax = originalAnchorMax;
            grid.pivot = originalPivot;

            grid.offsetMin = originalOffsetMin;
            grid.offsetMax = originalOffsetMax;

            Debug.Log("������������ ������������ ���������.");
            isMobileLayoutApplied = false;
        }
    }

    void CheckAspectRatio()
    {
        float aspectRatio = (float)Screen.width / Screen.height;

        // ���������, ���� ������ ������ ������ � ��������� 1.25�1 ���
        if (aspectRatio >= 1.0f && aspectRatio <= 1.25f)
        {
            // ��������� ������� �������� �� 0.4
            logo.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Debug.Log("������� ������� �������� �� 0.4.");
        }
        else
        {
            // ��������������� ������������ �������, ���� ����������� ������
            logo.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    // ����� ��� ���������� �����������: �������� ��� ��������
    public void ToggleKeyboard()
    {
        // ���������, ������� �� ����������
        if (mobileKeyboard == null || !mobileKeyboard.active)
        {
            // ���� ���������� ������� � ���������
            mobileKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            Debug.Log("�������� ���������� �������.");
        }
        else
        {
            // ���� ���������� ������� � ���������
            mobileKeyboard.active = false;
            Debug.Log("�������� ���������� ������.");
        }
    }

    void RaiseUIKeyboard()
    {
        // ��������� UI-���������� �� 50 �������� �� ��� Y
        keyboardUI.anchoredPosition = new Vector2(originalKeyboardPosition.x, originalKeyboardPosition.y + 50);
        Debug.Log("UI-���������� ������� �� 50 ��������.");
    }

    void RestoreUIKeyboardPosition()
    {
        // ��������������� ������������ ������� UI-����������
        keyboardUI.anchoredPosition = originalKeyboardPosition;
        Debug.Log("������������ ������� UI-���������� �������������.");
    }
}
