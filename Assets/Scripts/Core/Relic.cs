using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using RPNEvaluator;
using UnityEngine.InputSystem;

public class Relic
{
    public enum TriggerType
    {
        take_damage,
        stand_still,
        on_kill
    }
    public struct Trigger
    {
        public Trigger(JToken jsonConfig)
        {
            this.description = jsonConfig["description"].ToString();
            this.type = TriggerTypeFromString(jsonConfig["type"].ToString());
            this.amount = (jsonConfig["amount"] != null) ? jsonConfig["amount"].ToString() : "";
        }
        public string description { get; }
        public TriggerType type { get; }
        public string amount { get; }
        public override string ToString()
        {
            return this.description;
        }
    }
    public struct Effect
    {
        public Effect (JToken jsonConfig)
        {
            this.description = jsonConfig["description"].ToString();
            this.type = jsonConfig["type"].ToString();
            this.target_stat = jsonConfig["target_stat"].ToString();
            this.modification = jsonConfig["modification"].ToString();
            this.until = (jsonConfig["until"] != null) ? jsonConfig["until"].ToString() : "";
        }
        public string description { get; }
        public string type { get; }
        public string target_stat { get; }
        public string modification { get; }
        public string until { get; }

        public override string ToString()
        {
            return this.description;
        }
    }
    public string name { get; }
    public int sprite { get; }
    public float triggerTimeDelay { get; set; }
    public float lastTriggerTime { get; set; }
    public Trigger trigger { get; }
    public Effect effect { get; }
    private PlayerController owner { get; set; }
    public bool active { get; set; }

    public Action<Vector3, Damage, Hittable> OnDamage;
    public Action<Hittable> OnKill;
    public Action<ICastable> OnSpellReady;
    public Action<InputValue> OnMove;

    public Relic(JToken jsonConfig)
    {
        this.owner = null;

        this.name = jsonConfig["name"].ToString();
        this.sprite = (int)jsonConfig["sprite"];
        this.trigger = new Trigger(jsonConfig["trigger"]);
        this.effect = new Effect(jsonConfig["effect"]);

        this.triggerTimeDelay = -1.0f;
        this.lastTriggerTime = 0.0f;

        this.OnDamage = null;
        this.OnKill = null;
        this.OnSpellReady = null;
        this.OnMove = null;

        this.active = false;
    }

    public static TriggerType TriggerTypeFromString(string str)
    {
        TriggerType to_return;
        switch(str.ToLower())
        {
            case "take-damage":
                to_return = TriggerType.take_damage;
                break;
            case "stand-still":
                to_return = TriggerType.stand_still;
                break;
            default:
                to_return = TriggerType.on_kill;
                break;
        }
        return to_return;
    }
    public string GetOperationType()
    {
        switch(this.effect.type)
        {
            // add more cases in here if we add relics that do something other than addition
            default:
                return "+";
        }
    }
    public string GetTargetBaseStat()
    {
        switch(this.effect.target_stat)
        {
            case "power":
                return owner.spellcaster.spell_power.ToString();
            case "mana":
                return owner.spellcaster.mana.ToString();
            default:
                return "";
        }
    }

    public Func<Relic, float> GetEffectCall()
    {
        return (relic) => { 
            Dictionary<string, float> RPNDict = new Dictionary<string, float>();
            RPNDict.Add("wave", GameManager.Instance.GetWave());
            string ModExpr = relic.GetTargetBaseStat() + " " + relic.effect.modification + " " + relic.GetOperationType();
            if (this.effect.until == "")
            {
                Debug.Log(this.name + "deactivated");
                this.active = false;
            }
            return RPNEvaluator.RPNEvaluator.Evaluatef(ModExpr, RPNDict);
        };
    }

    public void BuildListeners()
    {
        switch(this.trigger.type)
        {
            case TriggerType.take_damage:
                this.OnDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == owner.hp.team)
                    {
                        Debug.Log(this.name + "activated");
                        this.active = true;
                    }
                };
                EventBus.Instance.OnDamage += OnDamage;
                break;
            case TriggerType.on_kill:
                this.OnKill = (killed) =>
                {
                    if (killed.team != owner.hp.team)
                    {
                        Debug.Log(this.name + "activated");
                        this.active = true;
                    }
                };
                EventBus.Instance.OnKill += OnKill;
                break;
            case TriggerType.stand_still:
                this.triggerTimeDelay = RPNEvaluator.RPNEvaluator.Evaluate(this.trigger.amount, new Dictionary<string, int>());
                break;
            default:
                break;
        }
        switch(this.effect.until)
        {
            case "cast-spell":
                OnSpellReady = (spell) => {
                    Debug.Log(this.name + "deactivated");
                    this.active = false;
                };
                break;
            case "move":
                OnMove = (value) => { this.lastTriggerTime = Time.time; };
                break;
            default:
                return;
        }
    }

    public void BindToOwner(PlayerController owner)
    {
        this.owner = owner;
        this.BuildListeners();
        // OnMove is bound by the owner
    }

    public override String ToString()
    {
        return "Name: " + name + "\nTrigger: " + trigger.ToString() + "\nEffect: " + effect.ToString();
    }
}
