using UnityEngine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

class Achievement
{
    public enum TrackType
    {
        CUMULATIVE,
        SINGLE_EVENT
    }
    TrackType TrackTypeFromString(string in_type)
    {
        switch(in_type.ToLower())
        {
            case "cumulative":
                return TrackType.CUMULATIVE;
            case "single_event":
                return TrackType.SINGLE_EVENT;
            default:
                throw new ArgumentException("TrackTypeFromString: Invalid track type \"" + in_type + "\"");
        }
    }

    public string name;
    public string description;

    string target_stat;
    string reset_trigger;
    TrackType track_type;

    float trigger_amount;
    string current_total;

    bool achieved;
    bool hasListeners;

    Action<string> DoTrack = null;
    public event Action<string, string> OnAchieved = null;

    public Action<Vector3, Damage, Hittable> OnDealDamage;
    public Action<Vector3, Damage, Hittable> OnTakeDamage;
    public Action<int> OnWaveStart;
    public Action<int> OnWaveEnd;
    public Action<Hittable> OnDeath;

    public Achievement(JToken jsonConfig)
    {
        this.name = jsonConfig[name].ToString();
        this.description = jsonConfig[description].ToString();
        this.target_stat = jsonConfig[target_stat].ToString();
        this.reset_trigger = jsonConfig[reset_trigger] != null ? jsonConfig[reset_trigger].ToString() : "none";
        this.track_type = TrackTypeFromString(jsonConfig[track_type].ToString());
        this.current_total = "0";
        this.achieved = false;
        this.BuildTrackCall();
        this.BuildListeners();
    }

    void BuildTrackCall()
    {
        this.DoTrack = (in_amount) =>
        {
            switch(this.track_type)
            {
                case TrackType.CUMULATIVE:
                    this.current_total = RPNEvaluator.RPNEvaluator.Evaluatef(this.current_total + " " + in_amount + " +", new Dictionary<string, float>()).ToString();
                    break;
                case TrackType.SINGLE_EVENT:
                    this.current_total = in_amount;
                    break;
                default: // this should never happen
                    return;
            }
            float amt = RPNEvaluator.RPNEvaluator.Evaluatef(this.current_total, new Dictionary<string, float>());
            if (amt >= this.trigger_amount)
            {
                this.GiveAchievement();
            }
        };
    }

    void BuildListeners()
    {
        if (this.hasListeners) return;
        this.hasListeners = true;
        // set stat tracking listener
        switch(this.target_stat.ToLower())
        {
            case "deal_damage":
                this.OnDealDamage = (where, damage, hittable) =>
                {
                    if (hittable.team != Hittable.Team.PLAYER)
                    {
                        this.DoTrack(damage.amount);
                    }
                };
                EventBus.Instance.OnDamage += OnDealDamage;
                break;
            case "take_damage":
                this.OnTakeDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == Hittable.Team.PLAYER)
                    {
                        this.DoTrack(damage.amount);
                    }
                };
                EventBus.Instance.OnDamage += OnTakeDamage;
                break;
            case "wave_start":
                this.OnWaveStart = (wave) =>
                {
                    this.DoTrack(wave.ToString());
                };
                EventBus.Instance.OnWaveStart += OnWaveStart;
                break;
            case "wave_end":
                this.OnWaveEnd = (wave) =>
                {
                    this.DoTrack(wave.ToString());
                };
                EventBus.Instance.OnWaveEnd += OnWaveEnd;
                break;
            default: // this should never happen
                return;
        }

        // set reset condition listener if applicable
        switch(this.reset_trigger.ToLower())
        {
            case "take_damage":
                this.OnTakeDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == Hittable.Team.PLAYER)
                    {
                        this.current_total = "0";
                    }
                };
                EventBus.Instance.OnDamage += OnTakeDamage;
                break;
            case "die":
                this.OnDeath = (hittable) =>
                {
                    if (hittable.team == Hittable.Team.PLAYER)
                    {
                        this.current_total = "0";
                    }
                };
                EventBus.Instance.OnKill += OnDeath;
                break;
            default: // should happen if reset_trigger == "none"
                return;
        }
    }

    void DestroyListeners()
    {
        if (!this.hasListeners) return;
        this.hasListeners = false;

        // kill stat tracking listener
        switch (this.target_stat.ToLower())
        {
            case "deal_damage":
                EventBus.Instance.OnDamage -= OnDealDamage;
                break;
            case "take_damage":
                EventBus.Instance.OnDamage -= OnTakeDamage;
                break;
            case "wave_start":
                EventBus.Instance.OnWaveStart -= OnWaveStart;
                break;
            case "wave_end":
                EventBus.Instance.OnWaveEnd -= OnWaveEnd;
                break;
            default: // this should never happen
                return;
        }

        // kill reset condition listener if applicable
        switch (this.reset_trigger.ToLower())
        {
            case "take_damage":
                EventBus.Instance.OnDamage -= OnTakeDamage;
                break;
            case "die":
                EventBus.Instance.OnKill -= OnDeath;
                break;
            default: // should happen if reset_trigger == "none"
                return;
        }
    }

    void GiveAchievement()
    {
        if (this.achieved) return;

        OnAchieved?.Invoke(this.name, this.description);
        this.achieved = true;

        this.DestroyListeners();
    }
}
