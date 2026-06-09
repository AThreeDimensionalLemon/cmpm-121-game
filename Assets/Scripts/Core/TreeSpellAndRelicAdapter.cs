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

    public void ApplyReward(string name, SkillTreeNode node = null) {
        SpellBuilder spellBuilder = SpellBuilder.Instance;
        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (spellBuilder.BaseSpells.ContainsKey(name)) {
            Spell newSpell = spellBuilder.BuildSpell(playerController.spellcaster, name);
            playerController.AddNewSpell(newSpell);
        }
        else if (spellBuilder.SpellModifiers.ContainsKey(name) && node != null) {
            SkillTreeNode prevNode = node;
            string prevName = prevNode.GetName();
            while (spellBuilder.BaseSpells.ContainsKey(prevName) || spellBuilder.SpellModifiers.ContainsKey(prevName)) {
                prevNode = prevNode.GetPrevNodes()[0];
                prevName = prevNode.GetName();
            }

            ICastable baseSpell = null;
            ICastable[] spells = playerController.spellcaster.spells;
            for (int i = 0; i < spells.Length; i++) {
                if (prevName == spells[i].GetName()) {
                    baseSpell = spells[i];
                    break;
                }
            }
            if (baseSpell == null) Debug.LogError("To-be-modified spell couldn't be found amongst player's spells");
            ModifiedSpell newSpell = spellBuilder.ModifySpell(playerController.spellcaster, baseSpell, name);
            playerController.AddNewSpell(newSpell);
        }
        else if (RelicManager.Instance.UnownedRelics.ContainsKey(name)) Debug.Log("Apply relic to player");
        else Debug.LogError("Invalid upgrade name received");
    }
}