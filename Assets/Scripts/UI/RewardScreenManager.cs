using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class RewardScreenManager : MonoBehaviour
{
    public GameObject rewardUI;

    public enum TextTypes
    {
        POSTWAVE,
        WIN,
        LOSS
    }
    public TextTypes text;

    private System.Collections.Generic.Dictionary<TextTypes, string> text_list = new System.Collections.Generic.Dictionary<TextTypes,string>();

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
                SetRewardScreenText(TextTypes.POSTWAVE);
                rewardUI.SetActive(true);
                rewardUI.transform.GetChild(1).GameObject().SetActive(true); // enable next wave button
                rewardUI.transform.GetChild(2).GameObject().SetActive(false); // disable restart button
                break;
            case GameManager.GameState.GAMEOVER:
                SetRewardScreenText(TextTypes.WIN);
                rewardUI.SetActive(true);
                rewardUI.transform.GetChild(1).GameObject().SetActive(false); // disable next wave button
                rewardUI.transform.GetChild(2).GameObject().SetActive(true); // enable restart button
                break;
            case GameManager.GameState.GAMELOST:
                SetRewardScreenText(TextTypes.LOSS);
                rewardUI.SetActive(true);
                rewardUI.transform.GetChild(1).GameObject().SetActive(false); // disable next wave button
                rewardUI.transform.GetChild(2).GameObject().SetActive(true); // enable restart button
                GameManager.Instance.KillAllEnemies();
                break;
            default:
                if (rewardUI.activeSelf)
                {
                    rewardUI.SetActive(false);
                }
                break;
        }
    }

    void SetRewardScreenText(TextTypes in_text)
    {
        PlayerStatisticsManager stats = GameManager.Instance.playerStatisticsManager;
        TextMeshProUGUI tmp = rewardUI.transform.GetChild(0).GameObject().GetComponent<TextMeshProUGUI>();
        tmp.text = text_list[in_text] + stats.GetStatisticsReadout();
    }

    
}
