using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class ModifiedSpell : ICastable
{
    public ICastable baseSpell;

    public string name;
    public string description;
    public Dictionary<string, string> modifications;

    private string TypeToOperator(string valueModType) {
        string v = valueModType.ToLower();
        switch (v) {
            case "multiplier": return "*";
            case "adder": return "+";
            default: throw new Exception("Invalid modifier type!");
        }
    }

    public ModifiedSpell(ICastable inBaseSpell, JToken modifierToken) {
        baseSpell = inBaseSpell;
        name = modifierToken["name"].ToString();
        description = modifierToken["name"].ToString();
        modifications = new Dictionary<string, string>();
        foreach (var valueMod in modifierToken["modifiers"]) {
            string value = valueMod["value"].ToString();
            string modification = valueMod["modification"].ToString() + " " + TypeToOperator(valueMod["type"].ToString());
            modifications.Add(value, modification);
        }
    }

    public string GetName() {
        return name + " " + baseSpell.GetName();
    }

    public int GetManaCost() {
        return baseSpell.GetManaCost();
    }

    public int GetDamage() {
        return baseSpell.GetDamage();
    }

    public float GetCooldown() {
        return baseSpell.GetCooldown();
    }

    public int GetIcon() {
        return baseSpell.GetIcon();
    }

    public bool IsReady() {
        return baseSpell.IsReady();
    }

    public float GetLastCast() {
        return baseSpell.GetLastCast();
    }

    public IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team) {
        return baseSpell.Cast(where, target, team);
    }
}
