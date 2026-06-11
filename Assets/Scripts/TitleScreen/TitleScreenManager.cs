using System.Linq;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Data;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Diagnostics.Tracing;
using UnityEngine.Audio;

public class TitleScreenManager : MonoBehaviour
{
    public GameObject PlayButton;
    public GameObject AchievementsButton;
    public GameObject ResetAchievementsButton;
    public GameObject CreditsButton;
    public GameObject BackButton;
    public GameObject Credits;
    public GameObject AchievementsDisplayWindow;
    public GameObject AchievementsDisplayContentArea;
    public GameObject AchievementDisplayBox;
    public GameObject TitleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.ViewMainMenu();
        this.BuildAchievementsReadout();
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Main");
    }

    public void ViewMainMenu()
    {
        TitleText.SetActive(true);
        PlayButton.SetActive(true);
        AchievementsButton.SetActive(true);
        AchievementsDisplayWindow.SetActive(false);
        CreditsButton.SetActive(true);
        Credits.SetActive(false);
        BackButton.SetActive(false);
        ResetAchievementsButton.SetActive(false);

        foreach (Transform achievementDisplayTransform in AchievementsDisplayContentArea.transform)
        {
            achievementDisplayTransform.gameObject.SetActive(false);
        }
    }

    public void ViewCredits()
    {
        TitleText.SetActive(false);
        PlayButton.SetActive(false);
        AchievementsButton.SetActive(false);
        ResetAchievementsButton.SetActive(false);
        AchievementsDisplayWindow.SetActive(false);
        CreditsButton.SetActive(false);
        Credits.SetActive(true);
        BackButton.SetActive(true);

        foreach (Transform achievementDisplayTransform in AchievementsDisplayContentArea.transform)
        {
            achievementDisplayTransform.gameObject.SetActive(false);
        }
    }

    public void ViewAchievements()
    {
        TitleText.SetActive(false);
        PlayButton.SetActive(false);
        AchievementsButton.SetActive(false);
        ResetAchievementsButton.SetActive(true);
        AchievementsDisplayWindow.SetActive(true);
        CreditsButton.SetActive(false);
        Credits.SetActive(false);
        BackButton.SetActive(true);

        UpdateAchievementsReadout();
        foreach (Transform achievementDisplayTransform in AchievementsDisplayContentArea.transform)
        {
            achievementDisplayTransform.gameObject.SetActive(true);
        }
    }

    public void ResetAchievements()
    {
        AchievementManager.Instance.ResetAchievements();
        UpdateAchievementsReadout();
    }

    void BuildAchievementsReadout()
    {
        float displayHeight = AchievementsDisplayContentArea.GetComponent<RectTransform>().rect.height;
        float achievementPaddingY = 12.5f;
        float currIndex = 0;
        float achDisplayHeight = -1;
        foreach (Achievement ach in AchievementManager.Instance.achievements.Values) {
            GameObject achDisplay = Instantiate(AchievementDisplayBox, AchievementsDisplayContentArea.transform);
            if (achDisplayHeight < 0) achDisplayHeight = achDisplay.GetComponent<RectTransform>().rect.height;

            achDisplay.transform.localPosition = new Vector3(0, displayHeight / 2 - (achDisplayHeight + achievementPaddingY * 2) * currIndex);
            TextMeshProUGUI achDisplayText = achDisplay.GetComponentInChildren<TextMeshProUGUI>();
            string ach_text = ach.name + " of " + ach.GetNumTiers() + "\n" + ach.description + "\n"; 
            if (ach.hasListeners)
            {
                ach_text += "Current Progress: " + ach.current_total + " of " + ach.target_amounts[ach.current_tier];
            }
            else
            {
                ach_text += "Maximum tier achieved!";
            }
            achDisplayText.text = ach_text;
            achDisplay.SetActive(false);
            currIndex++;
        }
        AchievementsDisplayContentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(
            AchievementsDisplayContentArea.GetComponent<RectTransform>().rect.width,
            (achDisplayHeight  + 2 * achievementPaddingY) * currIndex - achievementPaddingY);
    }

    void UpdateAchievementsReadout()
    {
        int currAchievementIndex = 0;
        foreach (Transform achievementDisplayTransform in AchievementsDisplayContentArea.transform)
        {
            Achievement ach = AchievementManager.Instance.achievements.ElementAt(currAchievementIndex).Value;
            TextMeshProUGUI achievementText = achievementDisplayTransform.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            string ach_text = ach.name + " of " + ach.GetNumTiers() + "\n" + ach.description + "\n";
            if (ach.hasListeners)
            {
                ach_text += "Current Progress: " + ach.current_total + " of " + ach.target_amounts[ach.current_tier];
            }
            else
            {
                ach_text += "Maximum tier achieved!";
            }
            achievementText.text = ach_text;
            currAchievementIndex++;
        }
    }
}
