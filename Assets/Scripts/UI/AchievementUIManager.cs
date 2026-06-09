using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using TMPro;

class AchievementUIManager : MonoBehaviour
{
    public GameObject achievementDisplay;
    private Image achievementUI;
    public GameObject achievementDisplayText;
    private TextMeshProUGUI achievementUI_TMP;
    private float displayTimer;
    private bool isFading;
    private const float START_FADEOUT_TIME = 3;
    private const float FADEOUT_DURATION = 2;

    struct AchievementDesc
    {
        public string name;
        public int tier;
        public string desc;
        public AchievementDesc(string name, int tier, string desc)
        {
            this.name = name;
            this.tier = tier;
            this.desc = desc;
        }
    }

    private List<AchievementDesc> toDisplay;
    void Start()
    {
        achievementUI = achievementDisplay.GetComponent<Image>();
        achievementUI_TMP = achievementDisplayText.GetComponent<TextMeshProUGUI>();
        toDisplay = new List<AchievementDesc>();
        displayTimer = 0;
        foreach (Achievement ach in AchievementManager.Instance.achievements.Values)
        {
            ach.OnAchieved += this.StartAchievementDisplay;
        }
        achievementDisplay.SetActive(false);
    }

    void Update()
    {
        if (toDisplay.Count > 0) this.UpdateAchievementDisplay();
    }

    void StartAchievementDisplay(string name, int tier, string desc)
    {
        toDisplay.Add(new AchievementDesc(name, tier, desc));
    }

    void UpdateAchievementDisplay()
    {
        if (!achievementDisplay.activeSelf)
        {
            achievementUI_TMP.text = "Achievement earned!\n" + toDisplay[0].name + "\n" + toDisplay[0].desc;
            achievementUI.canvasRenderer.SetAlpha(1);
            achievementUI_TMP.canvasRenderer.SetAlpha(1);
            achievementDisplay.SetActive(true);
        }
        else
        {
            displayTimer += Time.deltaTime;
            if (displayTimer > START_FADEOUT_TIME && !isFading)
            {
                achievementUI.CrossFadeAlpha(0, FADEOUT_DURATION, false);
                achievementUI_TMP.CrossFadeAlpha(0, FADEOUT_DURATION, false);
                isFading = true;
            }
            if (isFading && achievementUI.canvasRenderer.GetAlpha() < 0.001)
            {
                Debug.Log("fade over");
                toDisplay.RemoveAt(0);
                displayTimer = 0;
                isFading = false;
                achievementDisplay.SetActive(false);
            }
        }
    }
}