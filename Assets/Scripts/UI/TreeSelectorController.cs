using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Reflection.Emit;

class TreeSelectorController : MenuSelectorController {
    SkillTreeNode node;
    public GameObject icon;

    public void Setup(string text, SkillTreeNode inNode) {
        label.text = text;
        node = inNode;

        if (SpellBuilder.Instance.BaseSpells.ContainsKey(inNode.GetName())) {
            var spelljsondata = SpellBuilder.Instance.BaseSpells[inNode.GetName()];
            int iconIdx = spelljsondata["icon"].ToObject<int>();
            GameManager.Instance.spellIconManager.PlaceSprite(iconIdx, icon.GetComponent<Image>());
        }
        else if (RelicManager.Instance.GetAllRelics().ContainsKey(inNode.GetName())) {
            Relic tmp = RelicManager.Instance.GetRelic(inNode.GetName());
            GameManager.Instance.relicIconManager.PlaceSprite(tmp.sprite, icon.GetComponent<Image>());
        }

        if (SpellBuilder.Instance.SpellModifiers.ContainsKey(inNode.GetName()))
        {
            icon.SetActive(false); // no icon for modifiers (for now?)
        }
        else
        {
            // label.transform.SetAsLastSibling();
            label.transform.Translate(0,-55,0);  // put text below icon
        }
    }

    public void ExecuteTask() {
        TreeSpellAndRelicAdapter.Instance.ApplyReward(label.text, node);
        node.Take();
        UnityEngine.Debug.Log("clicked");
        gameObject.GetComponent<AudioSource>().Play();
    }
}
