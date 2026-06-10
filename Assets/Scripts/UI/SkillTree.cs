using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
public class SkillTree
{
    public SkillTreeNode baseNode;
    public SkillTree()
    {
        int branch_index_counter = -1;
        baseNode = new SkillTreeNode("base", branch_index_counter);

        // parse json and make skill tree!
        JObject parsedSkillTreeJson = JObject.Parse(Resources.Load<TextAsset>("skilltree").text);
        Dictionary<string, JToken> branches = new Dictionary<string, JToken>();

        foreach(KeyValuePair<string, JToken> branch in parsedSkillTreeJson)
        {
            branch_index_counter++;
            SkillTreeNode baseSpellNode = new SkillTreeNode(branch.Key, branch_index_counter);
            baseNode.AddNext(baseSpellNode);
            baseSpellNode.AddPrev(baseNode);
            baseSpellNode.SetIsExclusiveInBranchLevel(false);
            
            // initialize the previous branch level list of nodes to just have the base spell node
            List<SkillTreeNode> previousBranchLevel = new List<SkillTreeNode>();
            previousBranchLevel.Add(baseSpellNode);

            // iterate over all branch levels and set up their nodes and dependencies
            foreach(JToken branchLevel in branch.Value.Children())
            {
                // new list to hold all items in the current branch level
                List<SkillTreeNode> thisBranchLevel = new List<SkillTreeNode>();
                // iterate over everything in this branch level, make a new skill tree node for it, and add to list
                foreach(JToken branchItem in branchLevel.Children())
                {
                    SkillTreeNode newNode = new SkillTreeNode(branchItem.ToString(), branch_index_counter);
                    thisBranchLevel.Add(newNode);
                    // set the new node's previous nodes list
                    newNode.SetPrev(previousBranchLevel);
                    // make this node exclusive in branch level
                    newNode.SetIsExclusiveInBranchLevel(true);

                    // string str = newNode.GetName() + " previous nodes: [";
                    // foreach(SkillTreeNode n in newNode.GetPrevNodes())
                    // {
                    //     str += n.GetName() + ", ";
                    // }
                    // str += "]";
                    // Debug.Log(str);
                }
                // set the 'next node' list for all nodes in previous level to be thisBranchLevel
                foreach(SkillTreeNode prevNode in previousBranchLevel)
                {
                    prevNode.SetNext(thisBranchLevel);
                }
                // set prevousBranchLevel list to be thisBranchLevel so the next level has the proper items in previousBranchLevel
                previousBranchLevel = thisBranchLevel;
            }
        }

        StartGameState();
        // take base node and take arcane bolt base spell for starters
        // baseNode.Take();
        // baseNode.GetNextNodes()[0].Take();
        // THIS HAS BEEN MOVED TO REWARDSCREENMANAGER after it makes the buttons
    }

    public override string ToString()
    {
        string str = "";
        // go over the base spell nodes
        foreach(SkillTreeNode node in baseNode.GetNextNodes())
        {
            str += node.GetName() + ": [\n";
            List<SkillTreeNode> nextNodes = node.GetNextNodes();
            // go over each branch level for this base spell's branch
            while (nextNodes != null)
            {
                // go over each item in this branch level
                str += "[";
                foreach(SkillTreeNode nextNode in nextNodes)
                {
                    str += nextNode.GetName() + ", ";
                }
                str += "]\n";
                nextNodes = nextNodes[0].GetNextNodes(); // same for all nodes in list
            }
            str += "]\n";
        }
        str += "]\n";
        return str;
    }

    private void StartGameState()
    {
        // take base node and take arcane bolt base spell for starters
        baseNode.Take();
        baseNode.GetNextNodes()[0].Take();
    }

    public void Reset()
    {
        baseNode.SetIsTaken(false);
        baseNode.SetIsAvailable(false);
        // go over all nodes in tree after base node, set isTaken and isAvailable to false
        // go over the base spell nodes
        foreach(SkillTreeNode node in baseNode.GetNextNodes())
        {
            node.SetIsTaken(false);
            node.SetIsAvailable(false);
            List<SkillTreeNode> nextNodes = node.GetNextNodes();
            // go over each branch level for this base spell's branch
            while (nextNodes != null)
            {
                // go over each item in this branch level
                foreach(SkillTreeNode nextNode in nextNodes)
                {
                    nextNode.SetIsTaken(false);
                    nextNode.SetIsAvailable(false);
                }
                nextNodes = nextNodes[0].GetNextNodes(); // same for all nodes in list
            }
        }
        // enter state for start of game
        StartGameState();
    }
}