using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{
    private JToken parsedSpellsJson;
    private static SpellBuilder theInstance;

    public Spell BuildSpell(SpellCaster owner, string name)
    {
        return new Spell(owner, parsedSpellsJson[name]);
    }

    public static SpellBuilder Instance {
        get {
            if (theInstance == null) theInstance = new SpellBuilder();
            return theInstance;
        }
    }

   
    private SpellBuilder()
    {
        parsedSpellsJson = JToken.Parse(Resources.Load<TextAsset>("spells").text);
    }

}
