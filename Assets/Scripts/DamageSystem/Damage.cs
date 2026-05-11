using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System.Collections.Generic;
using UnityEngine;

public class Damage 
{
    public int amount;
    public enum Type
    {
        PHYSICAL, ARCANE, NATURE, FIRE, ICE, DARK, LIGHT
    }
    public Type type;
    public Damage(int inAmount, Damage.Type inType) {
        this.amount = inAmount;
        this.type = inType;
    }
    public Damage(JToken damageToken) //constructor used by spell
    {
        this.amount = RPNEvaluator.RPNEvaluator.Evaluate(damageToken["amount"].ToString(), new Dictionary<string, int> {
            { "power", 1 } //TODO: Figure out how to store player power and, subsequently, how to get it here
        });
        this.type = TypeFromString(damageToken["type"].ToString());
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
