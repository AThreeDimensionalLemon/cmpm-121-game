using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
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

    public void Take()
    {
        isTaken = true;
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