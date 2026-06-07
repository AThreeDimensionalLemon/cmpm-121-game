using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
public class SkillTreeNode
{
    private string name;
    private bool isTaken;
    private List<SkillTreeNode> nextNodes; // only rly need to parse in one direction for now, probably?
    public SkillTreeNode(string name)
    {
        this.name = name;
        isTaken = false;
    }

    public void setNext(List<SkillTreeNode> nodeList)
    {
        nextNodes = nodeList;
    }
    public void addNext(SkillTreeNode newNode)
    {
        if (nextNodes == null)
        {
            nextNodes = new List<SkillTreeNode>();
        }
        nextNodes.Add(newNode);
    }

    public string getName()
    {
        return name;
    }
    public List<SkillTreeNode> getNextNodes()
    {
        return nextNodes;
    }
}