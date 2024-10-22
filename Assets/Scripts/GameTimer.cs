using UnityEngine;
using TMPro;
using System.Diagnostics;

public class GameTimer : MonoBehaviour
{
    private Stopwatch stopwatch;
    public TMP_Text timerText;

    void Start()
    {
        stopwatch = new Stopwatch();
        timerText = GetComponent<TMP_Text>();
        StartGame();
    }

    void Update()
    {
        if (stopwatch.IsRunning)
        {
            UpdateTimerDisplay();
        }
    }

    public void StartGame()
    {
        stopwatch.Start();
    }

    public void StopGame()
    {
        stopwatch.Stop();
    }

    public void ResetGame()
    {
        stopwatch.Reset();
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        float elapsedTime = (float)stopwatch.Elapsed.TotalSeconds;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public int ElapsedSeconds
    {
        get { return (int)stopwatch.Elapsed.TotalSeconds; }
    }
}
