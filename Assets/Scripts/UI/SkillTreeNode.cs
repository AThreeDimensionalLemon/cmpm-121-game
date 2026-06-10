using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class SkillTreeNode
{
    private string name;
    private int branchIndex;
    private bool isTaken;
    private bool isAvailable;
    private bool isExclusiveInBranchLevel; // true if you can only take one thing in the branch level, false otherwise (false for base spells)
    private List<SkillTreeNode> nextNodes; 
    private List<SkillTreeNode> prevNodes; 
    public GameObject treeButton;   // reference gets set when RewardScreenManager does CreateSkillTreeButtons()
    public List<GameObject> precedingLines = new List<GameObject>();    // ^
    public SkillTreeNode(string name, int branchIndex)
    {
        this.name = name;
        this.branchIndex = branchIndex;
        isTaken = false;
    }

    // will set isTaken to true, and also update the availability of nodes:
    //  - all nodes in the same branch level are no longer available
    //  - all nodes in the next branch level are now available
    public void Take()
    {     
        Debug.Log("TAKING " + name);
        // TreeSpellAndRelicAdapter.Instance.ApplyReward(name, this);
        // Debug.Log("HFJDSHFJLKDSHLJGD");
        isTaken = true;
        if (prevNodes != null && isExclusiveInBranchLevel)
        {
            // iterate over all nodes in the same node branch as this one
            foreach(SkillTreeNode n in prevNodes[0].GetNextNodes())
            {
                // Debug.Log(n.GetName() + " NOT available");
                n.SetIsAvailable(false);
            }
        }
        if (nextNodes != null)
        {
            foreach(SkillTreeNode n in nextNodes)
            {
                // Debug.Log(n.GetName() + " IS available");
                n.SetIsAvailable(true);
            }
        }

        SetIsAvailable(false);
        EventBus.Instance.InvokeRewardClaimed(this);
    }

    public void SetButtonActive()
    {
        if (treeButton != null)
        {
            treeButton.GetComponent<Button>().interactable = isAvailable;
        }
    }


    // -- getters and setters --

    public string GetName()
    {
        return name;
    }

    public int GetBranchIndex()
    {
        return branchIndex;
    }

    public bool GetIsTaken()
    {
        return isTaken;
    }

    public bool GetIsAvailable()
    {
        return isAvailable;
    }

    public void SetIsAvailable(bool available)
    {
        isAvailable = available;
        SetButtonActive();
        foreach(GameObject l in precedingLines)
        {
            l.GetComponent<UILineRenderer>().color = available ? new Color(1f, 0.7f, 0f) : new Color(1f,0f,0f); // yellow if available, else red
        }
        // bad hack to figure out which one to set green, since lines only know one of their endpoints
        if (prevNodes != null && isTaken)
        {
            for (int j = 0; j < prevNodes.Count; j++)
            {
                if (prevNodes[j].isTaken && j < precedingLines.Count)
                {
                    precedingLines[j].GetComponent<UILineRenderer>().color = new Color(0f, 1f, 0f); // taken = green
                }
            }
        }
    }

    public bool GetIsExclusiveInBranchLevel()
    {
        return isExclusiveInBranchLevel;
    }

    public void SetIsExclusiveInBranchLevel(bool exclusiveInBranchLevel)
    {
        isExclusiveInBranchLevel = exclusiveInBranchLevel;
    }

    public void SetNext(List<SkillTreeNode> nodeList)
    {
        nextNodes = nodeList;
    }
    public void AddNext(SkillTreeNode newNode)
    {
        if (nextNodes == null)
        {
            nextNodes = new List<SkillTreeNode>();
        }
        nextNodes.Add(newNode);
    }

    public void SetPrev(List<SkillTreeNode> nodeList)
    {
        prevNodes = nodeList;
    }
    public void AddPrev(SkillTreeNode newNode)
    {
        if (prevNodes == null)
        {
            prevNodes = new List<SkillTreeNode>();
        }
        prevNodes.Add(newNode);
    }

    public List<SkillTreeNode> GetNextNodes()
    {
        return nextNodes;
    }
    public List<SkillTreeNode> GetPrevNodes()
    {
        return prevNodes;
    }
}