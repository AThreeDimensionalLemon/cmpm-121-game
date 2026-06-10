using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;


public class SpellBuilder 
{
    private JObject parsedSpellsJson;
    private Dictionary<string, JToken> base_spells;
    private Dictionary<string, JToken> spell_modifiers;
    private static SpellBuilder theInstance;

    public Dictionary<string, JToken> BaseSpells {
        get { return base_spells; }
    }

    public Dictionary<string, JToken> SpellModifiers {
        get { return spell_modifiers; }
    }

    public Spell BuildSpell(SpellCaster owner, string name)
    {
        Debug.Log("BuildSpell called");
        return new Spell(owner, base_spells[name]);
    }

    public ModifiedSpell ModifySpell(SpellCaster owner, ICastable target, string name) {
        return new ModifiedSpell(owner, target, spell_modifiers[name]);
    }

    // maxMods is an integer RPN expression with dict RPNDict.
    public ICastable BuildRandomSpell(SpellCaster owner, string maxMods, Dictionary<string, int> RPNDict)
    {
        var rand = new System.Random();
        ICastable randSpell = BuildSpell(owner, base_spells.Keys.ElementAt(rand.Next(base_spells.Count())));
        //int numModifiers = rand.Next(GameManager.Instance.GetWave() / 2);
        List<string> modsToApply = spell_modifiers.Keys.ToList();
        for (int i = 0; i < rand.Next(RPNEvaluator.RPNEvaluator.Evaluate(maxMods, RPNDict)); i++)
        {
            int index = rand.Next(modsToApply.Count);
            randSpell = ModifySpell(owner, randSpell, modsToApply[index]);
            modsToApply.RemoveAt(index);
        }
        return randSpell;
    }

    public static SpellBuilder Instance {
        get {
            if (theInstance == null) theInstance = new SpellBuilder();
            return theInstance;
        }
    }

    private SpellBuilder()
    {
        parsedSpellsJson = JObject.Parse(Resources.Load<TextAsset>("spells").text);
        base_spells = new Dictionary<string, JToken>();
        spell_modifiers = new Dictionary<string, JToken>();

        foreach(KeyValuePair<string, JToken> obj in parsedSpellsJson)
        {
            if (obj.Value["icon"] != null)
            {
                base_spells.Add(obj.Key, obj.Value);
            }
            else
            {
                spell_modifiers.Add(obj.Key, obj.Value);
            }
        }
    }

}
