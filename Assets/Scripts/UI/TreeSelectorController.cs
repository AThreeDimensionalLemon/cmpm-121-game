using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

class TreeSelectorController : MenuSelectorController {
    SkillTreeNode node;

    public void Setup(string text, SkillTreeNode inNode) {
        label.text = text;
        node = inNode;
    }

    public void ExecuteTask() {
        TreeSpellAndRelicAdapter.Instance.ApplyReward(label.text, node);
        UnityEngine.Debug.Log("clicked");
    }
}
