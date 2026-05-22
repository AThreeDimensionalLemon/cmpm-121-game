using UnityEngine;
using System;

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
    public event Action<ICastable> OnSpellReady;
    
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

    public void DoSpellReady(ICastable spell)
    {
        OnSpellReady?.Invoke(spell);
    }
}
