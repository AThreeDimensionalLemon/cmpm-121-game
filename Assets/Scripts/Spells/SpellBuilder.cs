using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


public class SpellBuilder 
{
    private JToken parsedSpellsJson;

    public Spell Build(SpellCaster owner)
    {
        return new Spell(owner);
    }

   
    public SpellBuilder()
    {
        parsedSpellsJson = JToken.Parse(Resources.Load<TextAsset>("spells").text);
    }

}
