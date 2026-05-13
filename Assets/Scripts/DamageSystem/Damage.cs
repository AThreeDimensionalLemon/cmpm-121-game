using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System.Collections.Generic;
using UnityEngine;

public class Damage 
{
    public string amount;
    public Dictionary<string, int> damage_dict;
    public enum Type
    {
        PHYSICAL, ARCANE, NATURE, FIRE, ICE, DARK, LIGHT
    }
    public Type type;
    public Damage(string inAmount, Damage.Type inType) {
        this.amount = inAmount;
        this.type = inType;
    }
    public Damage(JToken damageToken, Dictionary<string, int> damageDict) //constructor used by spell
    {
        this.amount = damageToken["amount"].ToString();
        this.type = TypeFromString(damageToken["type"].ToString());
        damage_dict = damageDict;
    }

    public static Type TypeFromString(string type)
    {
        string t = type.ToLower();
        if (t == "arcane") return Type.ARCANE;
        if (t == "nature") return Type.NATURE;
        if (t == "fire") return Type.FIRE;
        if (t == "ice") return Type.ICE;
        if (t == "dark") return Type.DARK;
        if (t == "light") return Type.LIGHT;
        return Type.PHYSICAL;
    }
}
