using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
public class SkillTree
{
    private SkillTreeNode baseNode;
    public SkillTree()
    {
        baseNode = new SkillTreeNode("base");

        // parse json and make skill tree!
        JObject parsedSkillTreeJson = JObject.Parse(Resources.Load<TextAsset>("skilltree").text);
        Dictionary<string, JToken> branches = new Dictionary<string, JToken>();

        foreach(KeyValuePair<string, JToken> obj in parsedSkillTreeJson)
        {
            SkillTreeNode baseSpellNode = new SkillTreeNode(obj.Key);
            baseNode.addNext(baseSpellNode);
            
            // initialize the previous branch level list of nodes to just have the base spell node
            List<SkillTreeNode> previousBranchLevel = new List<SkillTreeNode>();
            previousBranchLevel.Add(baseSpellNode);

            // iterate over all branch levels and set up their nodes and dependencies
            foreach(JToken branchLevel in obj.Value.Children())
            {
                // new list to hold all items in the current branch level
                List<SkillTreeNode> thisBranchLevel = new List<SkillTreeNode>();
                // iterate over everything in this branch level, make a new skill tree node for it, and add to list
                foreach(JToken branchItem in branchLevel.Children())
                {
                    SkillTreeNode newNode = new SkillTreeNode(branchItem.ToString());
                    thisBranchLevel.Add(newNode);
                }
                // set the 'next node' list for all nodes in previous level to be thisBranchLevel
                foreach(SkillTreeNode prevNode in previousBranchLevel)
                {
                    prevNode.setNext(thisBranchLevel);
                }
                // set prevousBranchLevel list to be thisBranchLevel so the next level has the proper items in previousBranchLevel
                previousBranchLevel = thisBranchLevel;
            }
        }
    }

    public override string ToString()
    {
        string str = "";
        // go over the base spell nodes
        foreach(SkillTreeNode node in baseNode.getNextNodes())
        {
            str += node.getName() + ": [\n";
            List<SkillTreeNode> nextNodes = node.getNextNodes();
            // go over each branch level for this base spell's branch
            while (nextNodes != null)
            {
                // go over each item in this branch level
                str += "[";
                foreach(SkillTreeNode nextNode in nextNodes)
                {
                    str += nextNode.getName() + ", ";
                }
                str += "]\n";
                nextNodes = nextNodes[0].getNextNodes(); // same for all nodes in list
            }
            str += "]\n";
        }
        str += "]\n";
        return str;
    }
}