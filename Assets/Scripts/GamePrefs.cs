using UnityEngine;

public static class GamePrefs
{
    private const string KeyHighScore  = "sg_high_score";
    private const string KeyTotalCoins = "sg_total_coins";

    public static int  GetHighScore()   => PlayerPrefs.GetInt(KeyHighScore, 0);
    public static int  GetTotalCoins()  => PlayerPrefs.GetInt(KeyTotalCoins, 0);

    public static void SaveHighScore(int score)
    {
        if (score > GetHighScore())
        {
            PlayerPrefs.SetInt(KeyHighScore, score);
            PlayerPrefs.Save();
        }
    }

    public static void AddCoins(int amount)
    {
        PlayerPrefs.SetInt(KeyTotalCoins, GetTotalCoins() + amount);
        PlayerPrefs.Save();
    }
}
