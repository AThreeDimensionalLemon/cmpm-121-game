using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Mono.Cecil;
using System;
using System.Linq;


public class SpellBuilder 
{
    private JObject parsedSpellsJson;
    private Dictionary<string, JToken> base_spells;
    private Dictionary<string, JToken> spell_modifiers;
    private static SpellBuilder theInstance;

    public Spell BuildSpell(SpellCaster owner, string name)
    {
        return new Spell(owner, base_spells[name]);
    }

    public ModifiedSpell ModifySpell(ICastable target, string name) {
        return new ModifiedSpell(target, spell_modifiers[name]);
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
