using JetBrains.Annotations;
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
    public Dictionary<string, StatModifier> modifications;

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
        modifications = modifierToken["modifiers"].ToObject<Dictionary<string, StatModifier>>();
    }

    public string GetName() {
        return name + " " + baseSpell.GetName();
    }

    private int GetModifiedResult(int baseValue, string valueName) {
        int result = baseValue;
        if (modifications.ContainsKey(valueName)) {
            int modification = (int)RPNEvaluator.RPNEvaluator.Evaluatef(modifications[valueName].modification, new Dictionary<string, float>());
            if (modifications[valueName].type.ToLower() == "multiplier") result *= modification;
            else result += modification;
        }
        return result;
    }

    private float GetModifiedResult(float baseValue, string valueName) { //curse you .NET 2.1
        float result = baseValue;
        if (modifications.ContainsKey(valueName)) {
            float modification = (float)RPNEvaluator.RPNEvaluator.Evaluatef(modifications[valueName].modification, new Dictionary<string, float>());
            if (modifications[valueName].type.ToLower() == "multiplier") result *= modification;
            else result += modification;
        }
        return result;
    }

    public int GetManaCost() {
        return GetModifiedResult(baseSpell.GetManaCost(), "mana");
    }

    public int GetDamage() {
        return GetModifiedResult(baseSpell.GetDamage(), "damage");
    }

    public float GetCooldown() {
        return GetModifiedResult(baseSpell.GetCooldown(), "cooldown");
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
