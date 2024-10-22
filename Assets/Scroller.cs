using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    public enum ScrollType
    {
        None,        // Отключить анимацию
        Automatic,   // Автоматическая прокрутка
        MouseFollow  // Прокрутка в зависимости от курсора
    }

    [SerializeField] private RawImage _img;
    [SerializeField] private float _speed = 0.02f;
    [SerializeField] private Vector2 _autoScrollSpeed = new Vector2(0.01f, 0.01f);
    [SerializeField] public ScrollType scrollType = ScrollType.None;

    private Vector2 _center;

    void Start()
    {
        // Определяем центр экрана как точку отсчета
        _center = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }

    void Update()
    {
        switch (scrollType)
        {
            case ScrollType.None:
                // Не делаем ничего
                break;

            case ScrollType.Automatic:
                // Автоматическая прокрутка
                AutoScroll();
                break;

            case ScrollType.MouseFollow:
                // Прокрутка в зависимости от положения курсора
                MouseFollowScroll();
                break;
        }
    }

    void AutoScroll()
    {
        // Автоматическое смещение фона
        _img.uvRect = new Rect(_img.uvRect.position + _autoScrollSpeed * Time.deltaTime, _img.uvRect.size);
    }

    void MouseFollowScroll()
    {
        // Получаем координаты курсора относительно центра экрана
        Vector2 mousePosition = new Vector2(Input.mousePosition.x - _center.x, Input.mousePosition.y - _center.y);

        // Нормализуем координаты
        Vector2 normalizedMousePosition = new Vector2(mousePosition.x / Screen.width, mousePosition.y / Screen.height);

        // Смещаем uvRect в зависимости от положения курсора
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
            // Если значение null, устанавливаем режим MouseFollow
            scrollType = ScrollType.MouseFollow;
        }
        else
        {
            // Если значение true, устанавливаем режим Automatic, иначе None
            scrollType = value.Value ? ScrollType.Automatic : ScrollType.None;
        }
    }

}
