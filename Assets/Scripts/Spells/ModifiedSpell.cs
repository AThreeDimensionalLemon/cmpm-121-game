using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Hittable;
using static UnityEngine.GraphicsBuffer;

public class ModifiedSpell : ICastable
{
    public ICastable baseSpell;

    public string spellName;
    public string description;
    public Dictionary<string, StatModifier> modifications;
    public Hittable.Team team;
    public SpellCaster owner;

    private string TypeToOperator(string valueModType) {
        string v = valueModType.ToLower();
        switch (v) {
            case "multiplier": return "*";
            case "adder": return "+";
            default: throw new Exception("Invalid modifier type!");
        }
    }

    public ModifiedSpell(SpellCaster inOwner, ICastable inBaseSpell, JToken modifierToken) {
        baseSpell = inBaseSpell;
        spellName = modifierToken["name"].ToString();
        description = modifierToken["name"].ToString();
        modifications = modifierToken["modifiers"].ToObject<Dictionary<string, StatModifier>>();
        owner = inOwner;
    }

    public string GetName() {
        return spellName + " " + baseSpell.GetName();
    }

    public string GetDescription()
    {
        return spellName + ": " + description + '\n' + baseSpell.GetDescription();
    }

    public SpellCaster GetOwner()
    {
        return baseSpell.GetOwner();
    }

    private int GetModifiedResult(int baseValue, string valueName) {
        Dictionary<string, float> RPNDict = new();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        float result = baseValue;
        if (modifications.ContainsKey(valueName)) {
            float modification = RPNEvaluator.RPNEvaluator.Evaluatef(modifications[valueName].modification, RPNDict);
            if (modifications[valueName].type.ToLower() == "multiplier") result *= modification;
            else result += modification;
        }
        return (int)result;
    }

    private float GetModifiedResult(float baseValue, string valueName) { //curse you .NET 2.1
        Dictionary<string, float> RPNDict = new();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        float result = baseValue;
        if (modifications.ContainsKey(valueName)) {
            float modification = RPNEvaluator.RPNEvaluator.Evaluatef(modifications[valueName].modification, RPNDict);
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

    public int GetSecondaryDamage()
    {
        return GetModifiedResult(baseSpell.GetSecondaryDamage(), "secondary_damage");
    }

    public void SetSecondaryDamage(int inDamage)
    {
        baseSpell.SetSecondaryDamage(inDamage);
    }

    public void SetDamageDicts(Dictionary<string, int> dict)
    {
        baseSpell.SetDamageDicts(dict);
    }

    public float GetCooldown() {
        return GetModifiedResult(baseSpell.GetCooldown(), "cooldown");
    }

    public int GetN()
    {
        return GetModifiedResult(baseSpell.GetN(), "N");
    }

    public int GetNumSplits()
    {
        return GetModifiedResult(baseSpell.GetNumSplits(), "num_splits");
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

    public void SetLastCast(float in_last_cast)
    {
        baseSpell.SetLastCast(in_last_cast);
    }

    public Damage.Type GetDamageType() {
        if (modifications.ContainsKey("damage_type")) return Damage.TypeFromString(modifications["damage_type"].ToString());
        else return baseSpell.GetDamageType();
    }

    public List<Projectile> GetProjectiles() { //TODO: Implement getting even more projectiles
        List<Projectile> result = baseSpell.GetProjectiles();
        return result;
    }

    public List<Vector3> GetTargetList(Vector3 origin, Vector3 direction, float angleRange, int targetAmount)
    {
        return baseSpell.GetTargetList(origin, direction, angleRange, targetAmount);
    }

    private string TypeToOperation(string inModifierType) {
        string m = inModifierType.ToLower();
        switch(m) {
            case "multiplier": return "*";
            default: return "+";
        }
    }

    public IEnumerator Cast(Vector3 where, List<Vector3> target, Hittable.Team inTeam, string lastModSpeed = null, Dictionary<string, float> lastModVariables = null, Action<Hittable, Vector3, int> InHitEvent = null, string lastTrajectory = null) {
        this.team = inTeam;

        //setting variables that aren't compile-time contants
        //variables
        Dictionary<string, float> variables = new() { { "power", owner.spell_power } };

        //add a target
        if (modifications.ContainsKey("angle")) {
            float angle = RPNEvaluator.RPNEvaluator.Evaluatef(modifications["angle"].modification, variables);
            target.AddRange(baseSpell.GetTargetList(where, target[0], angle, 1)); 
        }

        //speed mod
        string speedModification = (modifications.ContainsKey("speed")) ? modifications["speed"].modification + " " + TypeToOperation(modifications["speed"].type) : null;
        if (lastModSpeed != null && speedModification != null) speedModification += lastModSpeed;
        else if (lastModSpeed != null) speedModification = lastModSpeed;

        //others
        Action<Hittable, Vector3, int> HitEvent = (InHitEvent != null) ? InHitEvent : OnHit;
        string trajectory = (modifications.ContainsKey("trajectory")) ? modifications["trajectory"].modification : lastTrajectory;

        //handle delay
        if (modifications.ContainsKey("delay")) {
            float delay = RPNEvaluator.RPNEvaluator.Evaluatef(modifications["delay"].modification, variables);
            if (this.spellName == "whirlwind")
            {
                for (int i = 1; i < 10; i++)
                {
                    CoroutineManager.Instance.Run(DelayedCast(delay * i, speedModification, variables, HitEvent, trajectory));
                }
            } else {
                CoroutineManager.Instance.Run(DelayedCast(delay, speedModification, variables, HitEvent, trajectory));
            }
            //DelayedSpellCaster.Instance.DelayedCast(this.baseSpell, this.team, delay, where, target, speedModification, variables, HitEvent);
        }

        return baseSpell.Cast(where, target, this.team, speedModification, variables, HitEvent, trajectory);
    }

    void OnHit(Hittable other, Vector3 impact, int splits) {
        Debug.Log("modified OnHit");
        if (other.team != team) {
            if (splits == 0)
            {
                other.Damage(new Damage(this.GetDamage().ToString(), this.GetDamageType()));
            }
            else
            {
                Debug.Log("using secondary damage");
                other.Damage(new Damage(this.GetSecondaryDamage().ToString(), this.GetDamageType()));
            }
            if (splits < this.GetNumSplits())
            {
                Dictionary<string, int> RPNDictInt = new() { { "power", owner.spell_power } };
                Dictionary<string, float> RPNDictFloat = new() { { "power", owner.spell_power } };
                int intN = GetN();
                if (intN < 2)
                {
                    intN = 4;
                }
                Projectile splitProjectile = null;
                if (baseSpell.GetProjectiles().Count > 1)
                {
                    splitProjectile = baseSpell.GetProjectiles()[1];
                }
                else
                {
                    splitProjectile = baseSpell.GetProjectiles()[0];
                }
                if (baseSpell.GetSecondaryDamage() == 0)
                {
                    baseSpell.SetSecondaryDamage((int)((baseSpell.GetDamage() / (float)intN) * 2));
                }
                float speed = RPNEvaluator.RPNEvaluator.Evaluatef(splitProjectile.speed, RPNDictFloat);
                float lifetime = RPNEvaluator.RPNEvaluator.Evaluatef(splitProjectile.lifetime, RPNDictFloat);
                if (lifetime < 0.05f)
                {
                    lifetime = 0.5f;
                }
                foreach (Vector3 target in baseSpell.GetTargetList(impact, Vector3.right, 2 * (float)Math.PI, intN))
                {
                    GameManager.Instance.projectileManager.CreateProjectile(baseSpell.GetIcon(), splitProjectile.trajectory, impact, target - impact, speed, OnHit, lifetime, splits + 1, other);
                }
            }

            if (team == Hittable.Team.PLAYER) {
                GameManager.Instance.playerStatisticsManager.DamageDealt += this.GetDamage();
            }
        }
    }

    IEnumerator DelayedCast(float delay, string speedModification, Dictionary<string, float> variables, Action<Hittable, Vector3, int> HitEvent, string trajectory) {
        Debug.Log("beginning delayed cast");
        yield return new WaitForSeconds(delay);
        Debug.Log("casting delayed spell");
        Vector3 where = GameManager.Instance.player.transform.position;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        List<Vector3> target = new List<Vector3> { mouseWorld }; 
        yield return this.baseSpell.Cast(where, target, this.team, speedModification, variables, HitEvent, trajectory);
    }
}
