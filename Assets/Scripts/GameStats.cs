using System;
using System.Collections.Generic;
using System.Linq;

public static class GameStats
{
    public static int GamesPlayed { get; private set; }
    public static int Wins { get; private set; }
    public static int Losses => GamesPlayed - Wins;
    public static float WinPercentage => GamesPlayed > 0 ? (float)Math.Round((double)Wins / GamesPlayed * 100, 2) : 0f;
    public static int MaxScore => Scores.Count > 0 ? Scores.Max() : 0;
    public static float AverageScore => (float)(Scores.Count > 0 ? Scores.Average() : 0f);
    public static List<int> Scores { get; private set; } = new List<int>();


    public static void SetStats(int gamesPlayed, int wins, List<int> scores)
    {
        GamesPlayed = gamesPlayed;
        Wins = wins;
        Scores = new List<int>(scores);
    }
}
