using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using RPNEvaluator;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Buffers;

public class Relic
{
    public enum TriggerType
    {
        take_damage,
        deal_damage,
        stand_still,
        on_kill,
        wave_end,
        wave_start,
        max_health
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
            this.amount = (jsonConfig["amount"] != null) ? jsonConfig["amount"].ToString() : "";
        }
        public string description { get; }
        public string type { get; }
        public string target_stat { get; }
        public string modification { get; }
        public string until { get; }
        public string amount { get; }

        public override string ToString()
        {
            return this.description;
        }
    }
    public string name { get; }
    public int sprite { get; }
    public float triggerTimeDelay { get; set; }
    public float lastTriggerTime { get; set; }
    public float untilTimeDelay { get; set; }
    public Trigger trigger { get; }
    public Effect effect { get; }
    private PlayerController owner { get; set; }
    public bool active { get; set; }

    public Action<Vector3, Damage, Hittable> OnDamage;
    public Action<Hittable> OnKill;
    public Action<SpellCaster> OnSpellCast;
    public Action<float> OnMove;
    public Action<int> OnWaveEnd;
    public Action<int> OnWaveStart;

    Func<int, string> GetValueModifier = null;

    public Relic(JToken jsonConfig)
    {
        this.owner = null;

        this.name = jsonConfig["name"].ToString();
        this.sprite = (int)jsonConfig["sprite"];
        this.trigger = new Trigger(jsonConfig["trigger"]);
        this.effect = new Effect(jsonConfig["effect"]);

        this.triggerTimeDelay = -1.0f;
        this.lastTriggerTime = 0.0f;
        this.untilTimeDelay = -1.0f;

        this.OnDamage = null;
        this.OnKill = null;
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
            case "deal-damage":
                to_return = TriggerType.deal_damage;
                break;
            case "stand-still":
                to_return = TriggerType.stand_still;
                break;
            case "on-kill":
                to_return = TriggerType.on_kill;
                break;
            case "wave-end":
                to_return = TriggerType.wave_end;
                break;
            case "maxed-health":
                to_return = TriggerType.max_health;
                break;
            default:
                to_return = TriggerType.wave_start;
                break;
        }
        return to_return;
    }
    string GetOperationType()
    {
        switch(this.effect.type)
        {
            case "adder":
                return "+";
            case "multiplier":
                return "*";
            default: // this should never happen
                return "";
        }
    }

    void BuildEffectCall()
    {
        this.GetValueModifier = (base_value) => {
            Dictionary<string, float> RPNDict = new Dictionary<string, float>();
            RPNDict.Add("wave", GameManager.Instance.GetWave());
            return RPNEvaluator.RPNEvaluator.Evaluatef(this.effect.modification, RPNDict).ToString() + " " + this.GetOperationType();
        };

        switch (this.effect.target_stat)
        {
            case "spell_power":
                EventBus.Instance.OnGetSpellPower += this.GetValueModifier;
                break;
            case "mana":
                EventBus.Instance.OnGetMana += this.GetValueModifier;
                break;
            case "speed":
                EventBus.Instance.OnGetSpeed += this.GetValueModifier;
                break;
            case "player_hp":
                EventBus.Instance.OnGetPlayerHP += this.GetValueModifier;
                break;
            default:
                break;
        }
    }

    void BuildListeners()
    {
        switch(this.trigger.type)
        {
            case TriggerType.take_damage:
                this.OnDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == owner.hp.team)
                    {
                        this.Activate();
                    }
                };
                EventBus.Instance.OnDamage += OnDamage;
                break;
            case TriggerType.deal_damage:
                this.OnDamage = (where, damage, hittable) =>
                {
                    if (hittable.team != owner.hp.team)
                    {
                        this.Activate();
                    }
                };
                EventBus.Instance.OnDamage += OnDamage;
                break;
            case TriggerType.on_kill:
                this.OnKill = (killed) =>
                {
                    if (killed.team != owner.hp.team)
                    {
                        this.Activate();
                    }
                };
                EventBus.Instance.OnKill += OnKill;
                break;
            case TriggerType.stand_still:
                this.triggerTimeDelay = RPNEvaluator.RPNEvaluator.Evaluate(this.trigger.amount, new Dictionary<string, int>());
                this.lastTriggerTime = Time.time;
                break;
            case TriggerType.wave_end:
                this.OnWaveEnd = (wave) =>
                {
                    this.Activate();
                };
                EventBus.Instance.OnWaveEnd += OnWaveEnd;
                break;
            case TriggerType.wave_start:
                this.OnWaveStart = (wave) =>
                {
                    this.Activate();
                };
                EventBus.Instance.OnWaveStart += OnWaveStart;
                break;
            case TriggerType.max_health:
                // nothing here, player checks own hp in update()
                break;
            default: // this should never happen
                break;
        }
        switch(this.effect.until)
        {
            case "cast-spell":
                OnSpellCast = (spell) => {
                    this.Deactivate();
                };
                EventBus.Instance.OnSpellCast += OnSpellCast;
                break;
            case "move":
                OnMove = (value) => {
                    this.Deactivate();
                    this.lastTriggerTime = Time.time;
                };
                owner.unit.OnMove += OnMove;
                break;
            case "time-passed":
                this.untilTimeDelay = RPNEvaluator.RPNEvaluator.Evaluate(this.effect.amount, new Dictionary<string, int>());
                this.lastTriggerTime = Time.time - this.triggerTimeDelay;
                break;
            case "take-damage":
                OnDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == owner.hp.team)
                    {
                        this.Deactivate();
                    }
                };
                EventBus.Instance.OnDamage += OnDamage;
                break;
            default: // this.effect.until == ""
                break;
        }
    }

    void DestroyEffectCall()
    {
        if (this.GetValueModifier != null)
        {
            switch (this.effect.target_stat)
            {
                case "spell_power":
                    EventBus.Instance.OnGetSpellPower -= this.GetValueModifier;
                    break;
                case "mana":
                    EventBus.Instance.OnGetMana -= this.GetValueModifier;
                    break;
                case "speed":
                    EventBus.Instance.OnGetSpeed -= this.GetValueModifier;
                    break;
                case "player_hp":
                    EventBus.Instance.OnGetPlayerHP -= this.GetValueModifier;
                    break;
                default:
                    break;
            }
            this.GetValueModifier = null;
        }
    }

    void Fire()
    {
        switch (this.effect.target_stat)
        {
            case "mana":
                owner.spellcaster.mana = owner.spellcaster.mana;
                break;
            case "spell_power":
                owner.spellcaster.spell_power = owner.spellcaster.spell_power;
                break;
            case "player_hp":
                owner.hp.hp = owner.hp.hp;
                break;
            default:
                return;
        }
        this.Deactivate();
    }

    public void Activate()
    {
        this.lastTriggerTime = Time.time;
        Debug.Log(this.name + " activated");
        if (this.active) return;
        this.BuildEffectCall();
        this.active = true;
        if (this.effect.target_stat == "speed")
        {
            owner.unit.movement = owner.unit.movement.normalized * owner.speed;
        }
        if (this.effect.until == "") this.Fire();
    }

    public void Deactivate()
    {
        if (!this.active) return;
        Debug.Log(this.name + " deactivated");
        this.DestroyEffectCall();
        if (this.effect.target_stat == "speed")
        {
            owner.unit.movement = owner.unit.movement.normalized * owner.speed;
        }
        this.active = false;
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
