using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Hittable
{

    public enum Team { PLAYER, MONSTERS }
    public Team team;

    private int Hp;
    public int hp {
        get { 
            if (this.team == Team.PLAYER)
            {
                UnityEngine.Debug.Log("player hp: " + EventBus.Instance.GetPlayerHP(this, Hp));
                return EventBus.Instance.GetPlayerHP(this, Hp);
            }
            else
            {
                return Hp;
            }
        } 
        set { 
            Hp = value; 
        } 
    }

    public int max_hp;

    public GameObject owner;

    public void Damage(Damage damage)
    {
        EventBus.Instance.DoDamage(owner.transform.position, damage, this);
        hp -= RPNEvaluator.RPNEvaluator.Evaluate(damage.amount, damage.damage_dict);
        if (hp <= 0)
        {
            hp = 0;
            EventBus.Instance.DoKill(this);
            OnDeath();
        }
    }

    public event Action OnDeath;

    public Hittable(int hp, Team team, GameObject owner)
    {
        this.hp = hp;
        this.max_hp = hp;
        this.team = team;
        this.owner = owner;
    }

    public void SetMaxHP(int max_hp)
    {
        float perc = this.hp * 1.0f / this.max_hp;
        this.max_hp = max_hp;
        this.hp = Mathf.RoundToInt(perc * max_hp);
    }
}
