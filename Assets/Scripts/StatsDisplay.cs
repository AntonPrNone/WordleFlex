using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject statsWindow;
    public TMP_Text gamesPlayedText;
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text winPercentageText;
    public TMP_Text maxScoreText;
    public TMP_Text averageScoreText;

    private RectTransform statsRect;
    private RectTransform canvasRect;

    private bool isAnimating = false;

    void Start()
    {
        statsRect = statsWindow.GetComponent<RectTransform>();
        canvasRect = statsRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        statsWindow.SetActive(false);
    }

    public void UpdateStats()
    {
        gamesPlayedText.text = GameStats.GamesPlayed.ToString();
        winsText.text = GameStats.Wins.ToString();
        lossesText.text = GameStats.Losses.ToString();
        winPercentageText.text = $"{GameStats.WinPercentage}%";
        maxScoreText.text = GameStats.MaxScore.ToString();
        averageScoreText.text = GameStats.AverageScore.ToString();
    }

    public void ShowStats()
    {
        if (isAnimating) return;
        if (statsWindow.activeSelf)
        {
            HideStats();
            return;
        }

        UpdateStats();

        float startY = canvasRect.rect.height / 2 + statsRect.rect.height / 2;

        statsRect.anchoredPosition = new Vector2(statsRect.anchoredPosition.x, startY);

        statsWindow.SetActive(true);
        isAnimating = true;

        LeanTween.moveY(statsRect, 0f, 0.5f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            isAnimating = false;
        });
    }

    public void HideStats()
    {
        if (isAnimating) return;

        float endY = canvasRect.rect.height / 2 + statsRect.rect.height / 2;

        isAnimating = true;

        LeanTween.moveY(statsRect, endY, 0.5f).setEase(LeanTweenType.easeInBack).setOnComplete(() =>
        {
            statsWindow.SetActive(false);
            isAnimating = false;
        });
    }
}
