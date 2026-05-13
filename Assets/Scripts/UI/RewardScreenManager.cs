using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;
    public GameObject statsReadout;
    public GameObject nextWaveButton;
    public GameObject restartButton;
    public GameObject spellIconFrame;
    public GameObject spellIcon;
    public GameObject spellDescription;
    public GameObject takeSpellButton;
    public SpellUIContainer spellUI;


    private ICastable rewardSpell;
    public enum TextTypes
    {
        POSTWAVE,
        WIN,
        LOSS
    }
    public TextTypes text;

    private Dictionary<TextTypes, string> text_list = new Dictionary<TextTypes,string>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text_list.Add(TextTypes.WIN, "You win!\n\n\nGame Stats:\n");
        text_list.Add(TextTypes.LOSS, "You lose!\n\n\nGame Stats:\n");
        text_list.Add(TextTypes.POSTWAVE, "Wave destroyed!\n\n\nGame stats so far:\n");
    }

    // Update is called once per frame
    void Update()
    {
        switch(GameManager.Instance.state)
        {
            case GameManager.GameState.WAVEEND:
                if (!rewardUI.activeSelf)
                {
                    //SetRewardScreenText(TextTypes.POSTWAVE);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(true);
                    restartButton.SetActive(false);

                    GenerateSpellReward();
                    if (GameManager.Instance.player.GetComponent<PlayerController>().spellcaster.IsFull())
                    {
                        spellUI.ActivateDropButtons();
                    }
                    spellIconFrame.SetActive(true);
                    spellIcon.SetActive(true);
                    spellDescription.SetActive(true);
                    takeSpellButton.SetActive(true);
                }
                break;
            case GameManager.GameState.GAMEOVER:
                if (!rewardUI.activeSelf)
                {
                    SetRewardScreenText(TextTypes.WIN);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(false);
                    restartButton.SetActive(true);

                    spellIconFrame.SetActive(false);
                    spellIcon.SetActive(false);
                    spellDescription.SetActive(false);
                    takeSpellButton.SetActive(false);
                }
                break;
            case GameManager.GameState.GAMELOST:
                if (!rewardUI.activeSelf)
                {
                    SetRewardScreenText(TextTypes.LOSS);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(false);
                    restartButton.SetActive(true);
                    GameManager.Instance.KillAllEnemies();

                    spellIconFrame.SetActive(false);
                    spellIcon.SetActive(false);
                    spellDescription.SetActive(false);
                    takeSpellButton.SetActive(false);
                }
                break;
            default:
                if (rewardUI.activeSelf)
                {
                    rewardUI.SetActive(false);
                    spellUI.DeactivateDropButtons();
                }
                break;
        }
    }

    void GenerateSpellReward()
    {
        Dictionary<string, int> RPNDict = new();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        rewardSpell = SpellBuilder.Instance.BuildRandomSpell(GameManager.Instance.player.GetComponent<PlayerController>().spellcaster, "wave 2 / 1 +", RPNDict);
        GameManager.Instance.spellIconManager.PlaceSprite(rewardSpell.GetIcon(), spellIcon.GetComponent<Image>());
        spellDescription.GetComponent<TextMeshProUGUI>().text = rewardSpell.GetName() + "\n\n" + rewardSpell.GetDescription();
    }

    public void GiveSpellToPlayer()
    {
        if (GameManager.Instance.player.GetComponent<PlayerController>().AddNewSpell(rewardSpell))
        {
            takeSpellButton.SetActive(false);
            spellUI.DeactivateDropButtons();
        }
    }

    void SetRewardScreenText(TextTypes in_text)
    {
        PlayerStatisticsManager stats = GameManager.Instance.playerStatisticsManager;
        TextMeshProUGUI tmp = statsReadout.GetComponent<TextMeshProUGUI>();
        tmp.text = text_list[in_text] + stats.GetStatisticsReadout();
    }

    
}
