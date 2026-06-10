using UnityEngine;
using System;
using System.Collections.Generic;

public class EventBus
{
    private static EventBus theInstance;
    public static EventBus Instance
    {
        get
        {
            if (theInstance == null)
                theInstance = new EventBus();
            return theInstance;
        }
    }

    public event Action<Vector3, Damage, Hittable> OnDamage;
    public event Action<Relic> OnRelicPickup;
    public event Action<Hittable> OnKill;
    public event Action<SpellCaster> OnSpellCast;
    public event Func<float, string> OnGetSpellPower;
    public event Func<float, string> OnGetMana;
    public event Func<float, string> OnGetSpeed;
    public event Func<float, string> OnGetPlayerHP;
    public event Func<float, string> OnGetLastCast;
    public event Action<int> OnWaveEnd;
    public event Action<int> OnWaveStart;
    public event Action<SkillTreeNode> OnRewardClaimed;
    
    public void DoDamage(Vector3 where, Damage dmg, Hittable target)
    {
        OnDamage?.Invoke(where, dmg, target);
    }

    public void DoKill(Hittable killed)
    {
        OnKill?.Invoke(killed);
    }

    public void TakeRelic(Relic r)
    {
        OnRelicPickup?.Invoke(r);
    }

    public void DoSpellCast(SpellCaster caster)
    {
        OnSpellCast?.Invoke(caster);
    }

    public void DoWaveEnd(int wave)
    {
        OnWaveEnd?.Invoke(wave);
    }

    public void DoWaveStart(int wave)
    {
        OnWaveStart?.Invoke(wave);
    }

    public int GetSpellPower(int spell_power)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetSpellPower?.Invoke(spell_power));
        string to_ret = spell_power.ToString();
        foreach(string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluate(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return (int)RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>());
    }

    public int GetMana(SpellCaster caster, int mana)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetMana?.Invoke(mana));
        string to_ret = mana.ToString();
        foreach(string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluate(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return Math.Min((int)RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>()), caster.max_mana);
    }

    public int GetSpeed(int speed)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetSpeed?.Invoke(speed));
        string to_ret = speed.ToString();
        foreach (string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluate(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return (int)RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>());
    }

    public int GetPlayerHP(Hittable hittable, int hp)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetPlayerHP?.Invoke(hp));
        string to_ret = hp.ToString();
        foreach (string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluate(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return Math.Min((int)RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>()), hittable.max_hp);
    }

    public float GetLastCast(float last_cast)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetLastCast?.Invoke(last_cast));
        string to_ret = last_cast.ToString();
        foreach (string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluatef(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return RPNEvaluator.RPNEvaluator.Evaluatef(to_ret, new Dictionary<string, int>());
    }

    public void InvokeRewardClaimed(SkillTreeNode n)
    {
        OnRewardClaimed?.Invoke(n);
    }
}
