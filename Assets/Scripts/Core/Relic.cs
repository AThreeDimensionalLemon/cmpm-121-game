using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;

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
    public Trigger trigger { get; }
    public Effect effect { get; }

    public Relic(JToken jsonConfig)
    {
        this.name = jsonConfig["name"].ToString();
        this.sprite = (int)jsonConfig["sprite"];
        this.trigger = new Trigger(jsonConfig["trigger"]);
        this.effect = new Effect(jsonConfig["effect"]);
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

    public override String ToString()
    {
        return "Name: " + name + "\nTrigger: " + trigger.ToString() + "\nEffect: " + effect.ToString();
    }
}
