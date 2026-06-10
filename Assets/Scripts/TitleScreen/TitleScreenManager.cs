using UnityEngine;

public class TitleScreenManager : MonoBehaviour
{

    public GameObject PlayButton;
    public GameObject AchievementsButton;
    public GameObject CreditsButton;
    public GameObject BackButton;
    public GameObject Credits;
    public GameObject Achievements;
    public GameObject TitleText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.ViewMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ViewMainMenu()
    {
        TitleText.SetActive(true);
        PlayButton.SetActive(true);
        AchievementsButton.SetActive(true);
        Achievements.SetActive(false);
        CreditsButton.SetActive(true);
        Credits.SetActive(false);
        BackButton.SetActive(false);
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    public void ViewAchievements()
    {
        TitleText.SetActive(false);
        PlayButton.SetActive(false);
        AchievementsButton.SetActive(false);
        Achievements.SetActive(true);
        CreditsButton.SetActive(false);
        Credits.SetActive(false);
        BackButton.SetActive(true);
    }

    public void ViewCredits()
    {
        TitleText.SetActive(false);
        PlayButton.SetActive(false);
        AchievementsButton.SetActive(false);
        Achievements.SetActive(false);
        CreditsButton.SetActive(false);
        Credits.SetActive(true);
        BackButton.SetActive(true);
    }

}
