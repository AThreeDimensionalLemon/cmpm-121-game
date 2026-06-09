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

    void ApplySpell(string name) {

    }

    void ApplyModifier(string name) {

    }

    void ApplyRelic(string name) {

    }

    public static void ApplyReward(string name) {
        SpellBuilder spellBuilder = SpellBuilder.Instance;
        PlayerController playerController = GameManager.Instance.player.GetComponent<PlayerController>();
        if (spellBuilder.BaseSpells.ContainsKey(name)) {
            Spell newSpell = spellBuilder.BuildSpell(playerController.spellcaster, name);
            playerController.AddNewSpell(newSpell);
        }
        else if (spellBuilder.SpellModifiers.ContainsKey(name)) Debug.Log("Apply modifier to spell");
        else if (RelicManager.Instance.UnownedRelics.ContainsKey(name)) Debug.Log("Apply relic to player");
        else Debug.LogError("Invalid upgrade name received");
    }
}