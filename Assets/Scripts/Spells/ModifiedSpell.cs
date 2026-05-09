using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ModifiedSpell : ICastable
{
    private Spell baseSpell;

    public string name;
    public string description;
    public Dictionary<string, string> modifications;

    public ModifiedSpell(Spell inBaseSpell, JToken modifierToken) {
        baseSpell = inBaseSpell;
        name = modifierToken["name"].ToString();
        description = modifierToken["name"].ToString();
        modifications = new Dictionary<string, string>();
        foreach (var valueToken in modifierToken["modifiers"]) {

        }
    }

    public string GetName() {
        return name + baseSpell.GetName();
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

    public IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team) {
        return baseSpell.Cast(where, target, team);
    }
}
