using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

class AchievementManager
{
    private Dictionary<string, Achievement> achievements;
    private static AchievementManager theInstance;
    public static AchievementManager Instance
    {
        get
        {
            if (theInstance == null) theInstance = new AchievementManager();
            return theInstance;
        }
    }

    private AchievementManager()
    {
        achievements = new Dictionary<string, Achievement>();
        JToken parsedAchievementsJson = JToken.Parse(Resources.Load<TextAsset>("achievements").text);
        foreach (JToken in_achievement in parsedAchievementsJson)
        {
            Achievement achievement = new Achievement(in_achievement);
            achievements.Add(achievement.name, achievement);
        }
    }
}
