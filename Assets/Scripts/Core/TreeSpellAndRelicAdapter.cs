using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

/* Purpose: Adapter singleton between skill tree system and old spell and relic systems
 */
public class TreeSpellAndRelicAdapter {
    static TreeSpellAndRelicAdapter instance;

    public static TreeSpellAndRelicAdapter Instance {
        get {
            if (instance == null) instance = new();
            return instance;
        }
    }

//TODO: call applyreward when we take the first spell
    public void ApplyReward(string name, SkillTreeNode node) {
        SpellBuilder spellBuilder = SpellBuilder.Instance;
        RelicManager relicManager = RelicManager.Instance;
        Debug.Log(GameManager.Instance.player);
        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        Debug.Log(node.GetName());

        if (spellBuilder.BaseSpells.ContainsKey(name)) {
            Debug.Log(name + " is base spell");
            Spell newSpell = spellBuilder.BuildSpell(playerController.spellcaster, name);
            // Debug.Log("new spell: " + newSpell);
            playerController.AddNewSpell(newSpell);
            // Debug.Log(newSpell);
        }

        else if (spellBuilder.SpellModifiers.ContainsKey(name) && node != null) {
            Debug.Log(name + " is modifier spell");
            SkillTreeNode prevNode = node;
            string prevName = prevNode.GetName();
            while (spellBuilder.SpellModifiers.ContainsKey(prevName) || relicManager.GetAllRelics().ContainsKey(prevName)) {
                prevNode = prevNode.GetPrevNodes()[0];
                prevName = prevNode.GetName();
            }

            ICastable baseSpell = null;
            ICastable[] spells = playerController.spellcaster.spells;
            for (int i = 0; i < spells.Length; i++) {
                UnityEngine.Debug.Log("looking at spell " + i + " with name " + spells[i].GetName());
                Debug.Log(spells[i].GetName() + " vs " + prevName);
                if (spells[i] != null && prevName == spells[i].GetName()) {
                    Debug.Log("found!");
                    baseSpell = spells[i];
                    break;
                }
            }
            if (baseSpell == null) Debug.LogError("To-be-modified spell couldn't be found amongst player's spells");

            ModifiedSpell newSpell = spellBuilder.ModifySpell(playerController.spellcaster, baseSpell, name);

            playerController.AddNewSpell(newSpell);
        }

        else if (relicManager.UnownedRelics.ContainsKey(name)) {
            Relic newRelic = relicManager.GetRelic(name);
            EventBus.Instance.TakeRelic(newRelic);
            relicManager.UnownedRelics.Remove(newRelic.name);
        }

        else Debug.LogError("Invalid upgrade name received");
    }
}