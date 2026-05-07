using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class Spell 
{
    //spell stats
    private string name;
    private string description;
    private int icon;
    private Damage damage;
    private int manaCost;
    private int cooldown;
    private Projectile projectile;

    //other stuff
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner, JToken jsonConfig)
    {
        this.owner = owner;
    }

    public string GetName()
    {
        return name;
    }

    public int GetManaCost()
    {
        return manaCost;
    }

    public int GetDamage()
    {
        return damage.amount;
    }

    public float GetCooldown()
    {
        return cooldown;
    }

    public virtual int GetIcon()
    {
        return icon;
    }

    public bool IsReady()
    {
        return (last_cast + GetCooldown() < Time.time);
    }

    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team)
    {
        this.team = team;
        GameManager.Instance.projectileManager.CreateProjectile(0, "straight", where, target - where, 15f, OnHit);

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact)
    {
        if (other.team != team)
        {
            other.Damage(new Damage(GetDamage(), Damage.Type.ARCANE));
            if (team == Hittable.Team.PLAYER)
            {
                GameManager.Instance.playerStatisticsManager.DamageDealt += GetDamage();
            }
        }

    }

}
