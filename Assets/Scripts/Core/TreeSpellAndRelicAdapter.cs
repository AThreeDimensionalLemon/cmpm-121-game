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

    public static void ApplyReward(RewardScreenManager rewardScreenManager, string name) {
        SpellBuilder spellBuilder = SpellBuilder.Instance;
        RelicManager relicManager = RelicManager.Instance;
        if (spellBuilder.BaseSpells.ContainsKey(name)) rewardScreenManager.GiveSpellToPlayer(name);
        else if (spellBuilder.SpellModifiers.ContainsKey(name)) Debug.Log("Apply modifier to spell");
        else if (relicManager.UnownedRelics.ContainsKey(name)) Debug.Log("Apply relic to player");
        else Debug.LogError("Invalid upgrade name received");
    }
}