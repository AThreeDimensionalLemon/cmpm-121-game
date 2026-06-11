using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject mainMenuButton;
    public SpellUIContainer spellUI;
    public GameObject skillTreeUI;
    private SkillTree skillTreeData;
    public GameObject skillTreeNode;    // prefab
    public GameObject skillTreeLine;    // prefab
    private bool buttonsMade;
    public GameObject rewardClaimedPopup;

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
        buttonsMade = false;

        // CreateSkillTreeButtons();    // this gets called from PlayerController StartLevel now...

        EventBus.Instance.OnRewardClaimed += ShowClaimedScreen;
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
                    mainMenuButton.SetActive(false);
                    rewardClaimedPopup.SetActive(false);
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
                    mainMenuButton.SetActive(true);
                    rewardClaimedPopup.SetActive(false);
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
                    mainMenuButton.SetActive(true);
                    rewardClaimedPopup.SetActive(false);
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

    public void CreateSkillTreeButtons()
    {
        if (buttonsMade) return;
        buttonsMade = true;
        // attach to the moving "tree" background
        GameObject scrollableBG = skillTreeUI.transform.GetChild(0).gameObject;
        // GameObject test = Instantiate(skillTreeNode, scrollableBG.transform);

        int i = 0;
        int spacingBetweenBaseSpells = 275;
        int spacingBetweenLevels = 120;
        int spacingBetweenModOrRelic = 64;

        GameObject baseNode = Instantiate(skillTreeNode, scrollableBG.transform);
        skillTreeData.baseNode.treeButton = baseNode;
        var p = GameManager.Instance.player.GetComponent<PlayerController>();
        var playericon = GameManager.Instance.playerSpriteManager.Get(p.playerClass.sprite);
        baseNode.GetComponent<TreeSelectorController>().icon.GetComponent<Image>().sprite = playericon;
        
        // GameManager.Instance.playerSpriteManager.PlaceSprite(
        //     GameManager.Instance.player.GetComponent<PlayerController>().playerClass.sprite,
        //     baseNode.GetComponent<TreeSelectorController>().icon.GetComponent<Image>());    // this is getting out of hand
        baseNode.transform.localPosition += new UnityEngine.Vector3(spacingBetweenBaseSpells*1.5f, -spacingBetweenLevels, 0);   // does nothing, represents initial state

        // make the 4 base spell nodes off of the base node
        foreach(SkillTreeNode node in skillTreeData.baseNode.GetNextNodes())
        {
            // make and place the button
            GameObject baseSpellUINode = Instantiate(skillTreeNode, scrollableBG.transform);
            node.treeButton = baseSpellUINode;
            baseSpellUINode.transform.localPosition += new UnityEngine.Vector3(spacingBetweenBaseSpells*i, 0, 0);

            // draw line between new button and previous button
            GameObject lineObj = Instantiate(skillTreeLine, scrollableBG.transform);
            lineObj.transform.SetAsFirstSibling();    // so lines are under nodes
            node.precedingLines.Add(lineObj);
            node.SetIsAvailable(node.GetIsAvailable());  // kinda bad.. just to refresh the lines
            UILineRenderer line = lineObj.GetComponent<UILineRenderer>();
            int magicOffset = 50;
            line.points[0] = new Vector2(baseNode.transform.localPosition.x + magicOffset, baseNode.transform.localPosition.y + magicOffset);
            line.points[1] = new Vector2(baseSpellUINode.transform.localPosition.x + magicOffset, baseSpellUINode.transform.localPosition.y + magicOffset);

            // give the node a tree selector controller
            baseSpellUINode.GetComponent<TreeSelectorController>().Setup(node.GetName(), node);
            node.SetButtonActive();  // update buttons' active status
            
            int currentLevel = 1;
            List<SkillTreeNode> nextNodes = node.GetNextNodes();
            List<GameObject> prevNodeObjs = new List<GameObject>();
            prevNodeObjs.Add(baseSpellUINode);
            // go over each branch level for this base spell's branch
            while (nextNodes != null)
            {
                int currentLevelWidth = spacingBetweenModOrRelic * (nextNodes.Count-1);
                int j = 0;
                List<GameObject> currentNodeObjs = new List<GameObject>();  // ourghhhhhhhhh
                // go over each item in this branch level
                foreach(SkillTreeNode nextNode in nextNodes)
                {
                    GameObject modOrRelicNode = Instantiate(skillTreeNode, scrollableBG.transform);
                    nextNode.treeButton = modOrRelicNode;
                    int x = spacingBetweenBaseSpells*i - currentLevelWidth / 2 + spacingBetweenModOrRelic*j;
                    modOrRelicNode.transform.localPosition += new UnityEngine.Vector3(x, spacingBetweenLevels*currentLevel, 0);
                    j++;
                    currentNodeObjs.Add(modOrRelicNode);

                    foreach (GameObject prevNode in prevNodeObjs)
                    {
                        lineObj = Instantiate(skillTreeLine, scrollableBG.transform);
                        lineObj.transform.SetAsFirstSibling();
                        line = lineObj.GetComponent<UILineRenderer>();
                        line.points[0] = new Vector2(prevNode.transform.localPosition.x + magicOffset, prevNode.transform.localPosition.y + magicOffset);
                        line.points[1] = new Vector2(modOrRelicNode.transform.localPosition.x + magicOffset, modOrRelicNode.transform.localPosition.y + magicOffset);
                        nextNode.precedingLines.Add(lineObj);
                        nextNode.SetIsAvailable(nextNode.GetIsAvailable());
                    }
                    
                    modOrRelicNode.GetComponent<TreeSelectorController>().Setup(nextNode.GetName(), nextNode);
                    nextNode.SetButtonActive();
                }
                prevNodeObjs = new List<GameObject>(currentNodeObjs);
                nextNodes = nextNodes[0].GetNextNodes(); // same for all nodes in list
                currentLevel++;
            }
            i++;
        }

    }

    public void ResetSkillTree()
    {
        skillTreeData.Reset();
        skillTreeData.baseNode.Take();
        skillTreeData.baseNode.GetNextNodes()[0].Take();
    }

    // triggered from rewardclaimed event
    public void ShowClaimedScreen(SkillTreeNode node)
    {
        rewardClaimedPopup.SetActive(true);
        var rewardText = rewardClaimedPopup.transform.GetChild(0).GetComponent<Text>();

        //TODO: maybe the treespellandrelic adapter should have a general purpose translation method so i don't have to do this here
        string name = node.GetName();
        string desc = "\"" + name + "\" ";
        if (SpellBuilder.Instance.BaseSpells.ContainsKey(name)) {
            desc += " base spell:\n\n" + SpellBuilder.Instance.BaseSpells[name]["description"].ToObject<string>();
        }
        else if (SpellBuilder.Instance.SpellModifiers.ContainsKey(name) && node != null) {
            desc += " spell modifier:\n\n" + SpellBuilder.Instance.SpellModifiers[name]["description"].ToObject<string>();
        }
        else if (RelicManager.Instance.GetAllRelics().ContainsKey(name)) {
            desc += " relic:\n\n" + RelicManager.Instance.GetAllRelics()[name].trigger.description + ",\n" + RelicManager.Instance.GetAllRelics()[name].effect.description;
        }

        rewardText.text = desc;
    }
}
