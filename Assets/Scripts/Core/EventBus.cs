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
    public event Func<int, string> OnGetSpellPower;
    public event Func<int, string> OnGetMana;
    public event Func<int, string> OnGetSpeed;
    public event Action<int> OnWaveEnd;
    public event Action<int> OnWaveStart;
    
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
        return RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>());
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
        return Math.Min(RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>()), caster.max_mana);
    }

    public int GetSpeed(PlayerController player, int speed)
    {
        List<string> mods = new List<string>();
        mods.Add(OnGetSpeed?.Invoke(speed));
        string to_ret = speed.ToString();
        foreach (string mod in mods)
        {
            to_ret = RPNEvaluator.RPNEvaluator.Evaluate(to_ret + " " + mod, new Dictionary<string, int>()).ToString();
        }
        return RPNEvaluator.RPNEvaluator.Evaluate(to_ret, new Dictionary<string, int>());
    }
}
