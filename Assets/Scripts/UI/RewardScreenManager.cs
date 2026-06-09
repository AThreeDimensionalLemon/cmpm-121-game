using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    public SpellUIContainer spellUI;
    public GameObject skillTreeUI;
    private SkillTree skillTreeData;
    public GameObject skillTreeNode;    // prefab

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
        text_list.Add(TextTypes.POSTWAVE, "Wave destroyed!");

        // make skilltree object
        skillTreeData = new SkillTree();

        CreateSkillTreeButtons();
    }

    // Update is called once per frame
    void Update()
    {
        switch(GameManager.Instance.state)
        {
            case GameManager.GameState.WAVEEND:
                if (!rewardUI.activeSelf) {

                    SetRewardScreenText(TextTypes.POSTWAVE);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(true);
                    skillTreeUI.SetActive(true);
                    restartButton.SetActive(false);
                }
                break;
            case GameManager.GameState.GAMEOVER:
                if (!rewardUI.activeSelf)
                {
                    SetRewardScreenText(TextTypes.WIN);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(false);
                    skillTreeUI.SetActive(false);
                    restartButton.SetActive(true);
                }
                break;
            case GameManager.GameState.GAMELOST:
                if (!rewardUI.activeSelf)
                {
                    SetRewardScreenText(TextTypes.LOSS);
                    rewardUI.SetActive(true);
                    nextWaveButton.SetActive(false);
                    skillTreeUI.SetActive(false);
                    restartButton.SetActive(true);
                    GameManager.Instance.KillAllEnemies();
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

    void SetRewardScreenText(TextTypes in_text)
    {
        PlayerStatisticsManager stats = GameManager.Instance.playerStatisticsManager;
        TextMeshProUGUI tmp = statsReadout.GetComponent<TextMeshProUGUI>();
        tmp.text = text_list[in_text];
        if (in_text != TextTypes.POSTWAVE) {
            statsReadout.SetActive(true);
            tmp.text += stats.GetStatisticsReadout();
        }
        else statsReadout.SetActive(false);
    }

    void CreateSkillTreeButtons()
    {
        // attach to the moving "tree" background
        GameObject scrollableBG = skillTreeUI.transform.GetChild(0).gameObject;
        // GameObject test = Instantiate(skillTreeNode, scrollableBG.transform);

        int i = 0;
        int spacingBetweenBaseSpells = 250;
        int spacingBetweenLevels = 100;
        int spacingBetweenModOrRelic = 60;

        GameObject baseNode = Instantiate(skillTreeNode, scrollableBG.transform);
        baseNode.transform.localPosition += new UnityEngine.Vector3(spacingBetweenBaseSpells*1.5f, -spacingBetweenLevels, 0);   // does nothing, represents initial state

        // make the 4 base spell nodes off of the base node
        foreach(SkillTreeNode node in skillTreeData.baseNode.GetNextNodes())
        {
            // make and place the button
            GameObject baseSpellUINode = Instantiate(skillTreeNode, scrollableBG.transform);
            node.treeButton = baseSpellUINode;
            baseSpellUINode.transform.localPosition += new UnityEngine.Vector3(spacingBetweenBaseSpells*i, 0, 0);

            //TODO: draw line between new button and previous button

            // give the node a tree selector controller
            baseSpellUINode.GetComponent<TreeSelectorController>().Setup(node.GetName(), node);
            node.SetButtonActive();  // update buttons' active status
            
            int currentLevel = 1;
            List<SkillTreeNode> nextNodes = node.GetNextNodes();
            // go over each branch level for this base spell's branch
            while (nextNodes != null)
            {
                int currentLevelWidth = spacingBetweenModOrRelic * (nextNodes.Count-1);
                int j = 0;
                // go over each item in this branch level
                foreach(SkillTreeNode nextNode in nextNodes)
                {
                    GameObject modOrRelicNode = Instantiate(skillTreeNode, scrollableBG.transform);
                    nextNode.treeButton = modOrRelicNode;
                    int x = spacingBetweenBaseSpells*i - currentLevelWidth / 2 + spacingBetweenModOrRelic*j;
                    modOrRelicNode.transform.localPosition += new UnityEngine.Vector3(x, spacingBetweenLevels*currentLevel, 0);
                    j++;

                    modOrRelicNode.GetComponent<TreeSelectorController>().Setup(nextNode.GetName(), nextNode);
                    nextNode.SetButtonActive();
                }
                nextNodes = nextNodes[0].GetNextNodes(); // same for all nodes in list
                currentLevel++;
            }
            i++;
        }
    }
}
