using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

class AchievementManager
{
    public Dictionary<string, Achievement> achievements { get; }
    private static AchievementManager theInstance;
    public void Initialize()
    {
        if (theInstance == null) theInstance = new AchievementManager();
    }

    public void ResetAchievements()
    {
        foreach(Achievement ach in achievements.Values)
        {
            ach.ResetProgress();
        }
    }

    public void ResetAchievementListeners()
    {
        foreach(Achievement ach in achievements.Values)
        {
            ach.ResetListeners();
        }
    }

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
