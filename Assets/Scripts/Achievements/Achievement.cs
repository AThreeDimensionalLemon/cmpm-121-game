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

    private string Name;
    public string name
    {
        get
        {
            if (this.current_tier >= this.target_amounts.Count)
            {
                return "\"" + this.Name + "\" - Tier " + this.target_amounts.Count;
            }
            return "\"" + this.Name + "\" - Tier " + (this.current_tier + 1).ToString();
        }
        set
        {
            this.Name = value;
        }
    }
    private string Description;
    public string description { 
        get
        {
            string[] parsed_desc = this.Description.Split("$");
            if (this.current_tier >= this.target_amounts.Count)
            {
                return parsed_desc[0] + this.target_amounts[this.target_amounts.Count - 1].ToString() + parsed_desc[1];
            }
            return parsed_desc[0] + this.target_amounts[this.current_tier].ToString() + parsed_desc[1];
        }
        set
        {
            this.Description = value;
        }
    }

    string target_stat;
    string reset_trigger;
    TrackType track_type;

    public List<float> target_amounts { get; private set; }
    public int GetNumTiers()
    {
        return target_amounts.Count;
    }

    public string current_total { get; private set; }
    public int current_tier { get; private set; }

    public bool hasListeners { get; private set; }

    Action<string> DoTrack = null;
    public event Action<string, int, string> OnAchieved = null;

    public Action<Vector3, Damage, Hittable> OnDealDamage;
    public Action<Vector3, Damage, Hittable> OnTakeDamage;
    public Action<int> OnWaveStart;
    public Action<int> OnWaveEnd;
    public Action<Hittable> OnDeath;

    public Achievement(JToken jsonConfig)
    {
        this.name = jsonConfig["name"].ToString();
        this.description = jsonConfig["description"].ToString();
        this.target_stat = jsonConfig["target_stat"].ToString();
        this.target_amounts = new List<float>();
        foreach(float amt in jsonConfig["target_amounts"])
        {
            this.target_amounts.Add(amt);
        }
        this.reset_trigger = (jsonConfig["reset_trigger"] != null) ? jsonConfig["reset_trigger"].ToString() : "none";
        this.track_type = TrackTypeFromString(jsonConfig["track_type"].ToString());
        this.current_total = "0";
        this.current_tier = 0;
        // Debug.Log(this.name + "\n" + this.description);
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
            if (amt >= this.target_amounts[this.current_tier])
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
                        this.DoTrack(RPNEvaluator.RPNEvaluator.Evaluate(damage.amount, damage.damage_dict).ToString());
                    }
                };
                EventBus.Instance.OnDamage += OnDealDamage;
                break;
            case "take_damage":
                this.OnTakeDamage = (where, damage, hittable) =>
                {
                    if (hittable.team == Hittable.Team.PLAYER)
                    {
                        this.DoTrack(RPNEvaluator.RPNEvaluator.Evaluate(damage.amount, damage.damage_dict).ToString());
                    }
                };
                EventBus.Instance.OnDamage += OnTakeDamage;
                break;
            case "wave_start":
                this.OnWaveStart = (wave) =>
                {
                    this.DoTrack("1");
                };
                EventBus.Instance.OnWaveStart += OnWaveStart;
                break;
            case "wave_end":
                this.OnWaveEnd = (wave) =>
                {
                    this.DoTrack("1");
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
            case "run_end":
                this.OnWaveEnd = (wave) =>
                {
                    if (GameManager.Instance.state == GameManager.GameState.GAMEOVER)
                    {
                        this.current_total = "0";
                    }
                };
                EventBus.Instance.OnWaveEnd += OnWaveEnd;
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
            case "run_end":
                EventBus.Instance.OnWaveEnd -= OnWaveEnd;
                EventBus.Instance.OnKill -= OnDeath;
                break;
            default: // should happen if reset_trigger == "none"
                return;
        }
    }

    public void ResetListeners()
    {
        if (this.hasListeners) this.DestroyListeners();
        if (this.current_tier < this.target_amounts.Count) this.BuildListeners();
    }

    void GiveAchievement()
    {
        OnAchieved?.Invoke(this.name, this.current_tier + 1, this.description);

        // Debug.Log("Achievement get: " + this.name + "\n" + this.description);
        this.current_tier++;

        if (this.current_tier >= this.target_amounts.Count) this.DestroyListeners();
    }

    public void ResetProgress()
    {
        this.current_tier = 0;
        this.current_total = "0";
        if (!this.hasListeners) this.BuildListeners();
    }
}
