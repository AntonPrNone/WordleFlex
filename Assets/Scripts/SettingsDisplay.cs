using Unity.VisualScripting;
using UnityEngine;

public class SettingsDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject settingsWindow;

    public UISwitcher.UISwitcher saveWordWinSwitch;
    public UISwitcher.UISwitcher saveWordLoseSwitch;
    public UISwitcher.UISwitcher animBG;

    public Scroller scroller;

    private RectTransform settingsRect;
    private RectTransform canvasRect;

    private bool isAnimating = false;

    void Start()
    {
        settingsRect = settingsWindow.GetComponent<RectTransform>();
        canvasRect = settingsRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        settingsWindow.SetActive(false);

        InitializeSwitches();
    }

    private void InitializeSwitches()
    {
        // Получаем значения из базы данных настроек и переключаем переключатели
        saveWordWinSwitch.isOn = DatabaseManager.GetGameSettings().SaveWordWin;
        saveWordLoseSwitch.isOn = DatabaseManager.GetGameSettings().SaveWordLose;
        animBG.isOnNullable = DatabaseManager.GetGameSettings().AnimBG;

        scroller.SetScrollTypeFromNullableBool(DatabaseManager.GetGameSettings().AnimBG);
    }

    public void ShowSettings()
    {
        if (isAnimating) return;
        if (settingsWindow.activeSelf)
        {
            HideSettings();
            return;
        }

        float startY = canvasRect.rect.height / 2 + settingsRect.rect.height / 2;

        settingsRect.anchoredPosition = new Vector2(settingsRect.anchoredPosition.x, -startY);

        settingsWindow.SetActive(true);
        isAnimating = true;

        LeanTween.moveY(settingsRect, 0f, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            isAnimating = false;
        });
    }

    public void HideSettings()
    {
        if (isAnimating) return;

        float endY = canvasRect.rect.height / 2 + settingsRect.rect.height / 2;

        isAnimating = true;

        LeanTween.moveY(settingsRect, -endY, 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            settingsWindow.SetActive(false);
            isAnimating = false;
        });
    }

    public void ToggleSwitchAnimBG()
    {
        if (animBG.isOnNullable == null)
        {
            scroller.SetScrollType(Scroller.ScrollType.MouseFollow);
        }

        else if (animBG.isOnNullable == false)
        {
            scroller.SetScrollType(Scroller.ScrollType.None);
        }

        else if (animBG.isOnNullable == true)
        {
            scroller.SetScrollType(Scroller.ScrollType.Automatic);
        }

        DatabaseManager.SaveGameSettings(animBG: scroller.GetScrollTypeAsNullableBool());
    }
}
