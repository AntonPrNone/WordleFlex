using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    public enum ScrollType
    {
        None,        // ��������� ��������
        Automatic,   // �������������� ���������
        MouseFollow  // ��������� � ����������� �� �������
    }

    [SerializeField] private RawImage _img;
    [SerializeField] private float _speed = 0.02f;
    [SerializeField] private Vector2 _autoScrollSpeed = new Vector2(0.01f, 0.01f);
    [SerializeField] public ScrollType scrollType = ScrollType.None;

    private Vector2 _center;

    void Start()
    {
        // ���������� ����� ������ ��� ����� �������
        _center = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        switch (scrollType)
        {
            case ScrollType.None:
                // �� ������ ������
                break;

            case ScrollType.Automatic:
                // �������������� ���������
                AutoScroll();
                break;

            case ScrollType.MouseFollow:
                // ��������� � ����������� �� ��������� �������
                MouseFollowScroll();
                break;
        }
    }

    void AutoScroll()
    {
        // �������������� �������� ����
        _img.uvRect = new Rect(_img.uvRect.position + _autoScrollSpeed * Time.deltaTime, _img.uvRect.size);
    }

    void MouseFollowScroll()
    {
        // �������� ���������� ������� ������������ ������ ������
        Vector2 mousePosition = new Vector2(Input.mousePosition.x - _center.x, Input.mousePosition.y - _center.y);

        // ����������� ����������
        Vector2 normalizedMousePosition = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);

        // ������� uvRect � ����������� �� ��������� �������
        _img.uvRect = new Rect(normalizedMousePosition * _speed, _img.uvRect.size);
    }

    public void SetScrollType(ScrollType type)
    {
        scrollType = type;
    }

    public bool? GetScrollTypeAsNullableBool()
    {
        switch (scrollType)
        {
            case ScrollType.None:
                return false;
            case ScrollType.Automatic:
                return true;
            case ScrollType.MouseFollow:
                return null;
            default:
                return null;
        }
    }

    public void SetScrollTypeFromNullableBool(bool? value)
    {
        if (value == null)
        {
            // ���� �������� null, ������������� ����� MouseFollow
            scrollType = ScrollType.MouseFollow;
        }
        else
        {
            // ���� �������� true, ������������� ����� Automatic, ����� None
            scrollType = value.Value ? ScrollType.Automatic : ScrollType.None;
        }
    }

}
