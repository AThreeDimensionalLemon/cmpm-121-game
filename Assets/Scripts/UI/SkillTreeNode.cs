using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class SkillTreeNode
{
    private string name;
    private bool isTaken;
    private bool isAvailable;
    private bool isExclusiveInBranchLevel; // true if you can only take one thing in the branch level, false otherwise (false for base spells)
    private List<SkillTreeNode> nextNodes; 
    private List<SkillTreeNode> prevNodes; 
    public GameObject treeButton;   // reference gets set when RewardScreenManager does CreateSkillTreeButtons()
    public SkillTreeNode(string name)
    {
        this.name = name;
        isTaken = false;
    }

    // will set isTaken to true, and also update the availability of nodes:
    //  - all nodes in the same branch level are no longer available
    //  - all nodes in the next branch level are now available
    public void Take()
    {     
        Debug.Log("TAKING " + name);
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