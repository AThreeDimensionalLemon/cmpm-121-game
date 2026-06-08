using System.Collections.Generic;
public class SkillTreeNode
{
    private string name;
    private bool isTaken;
    private bool isAvailable;
    private List<SkillTreeNode> nextNodes; 
    private List<SkillTreeNode> prevNodes; 
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
        isTaken = true;
        if (prevNodes != null)
        {
            // iterate over all nodes in the same node branch as this one
            foreach(SkillTreeNode n in prevNodes[0].GetNextNodes())
            {
                n.SetIsAvailable(false);
            }
            foreach(SkillTreeNode n in nextNodes)
            {
                n.SetIsAvailable(true);
            }
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