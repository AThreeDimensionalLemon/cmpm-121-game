using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
public class SkillTreeNode
{
    private string name;
    private bool isTaken;
    private List<SkillTreeNode> nextNodes; 
    private List<SkillTreeNode> prevNodes; 
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

    public void setPrev(List<SkillTreeNode> nodeList)
    {
        prevNodes = nodeList;
    }
    public void addPrev(SkillTreeNode newNode)
    {
        if (prevNodes == null)
        {
            prevNodes = new List<SkillTreeNode>();
        }
        prevNodes.Add(newNode);
    }

    public string getName()
    {
        return name;
    }
    public List<SkillTreeNode> getNextNodes()
    {
        return nextNodes;
    }
    public List<SkillTreeNode> getPrevNodes()
    {
        return prevNodes;
    }
}