using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public class Spell : ICastable
{
    //spell stats
    private string name;
    private string description;
    private int icon;
    private string N; //number of projectiles
    private string spray; //angle of projectiles' launch, if N > 1
    private Damage damage;
    private string secondary_damage;
    private string mana_cost;
    private string cooldown;
    private Projectile projectile;
    private Projectile secondary_projectile;

    //other stuff
    public float last_cast;
    public SpellCaster owner;
    public Hittable.Team team;

    public Spell(SpellCaster owner, JToken jsonConfig) { //assigning manually because some fields not in JSON and complex storage of projectile info
        this.owner = owner;
        this.name = jsonConfig["name"].ToString();
        this.description = jsonConfig["description"].ToString();
        //this.icon = jsonConfig["icon"].ToObject<int>();
        this.icon = 0;
        this.N = (jsonConfig["N"] != null) ? jsonConfig["N"].ToString() : "0"; //if the key doesn't exist, use default of "0"
        this.spray = (jsonConfig["spray"] != null) ? jsonConfig["spray"].ToString() : "0";
        this.damage = new Damage(jsonConfig["damage"]);
        this.secondary_damage = (jsonConfig["secondary_damage"] != null) ? jsonConfig["secondary_damage"].ToString() : "0";
        this.mana_cost = jsonConfig["mana_cost"].ToString();
        this.cooldown = jsonConfig["cooldown"].ToString();
        this.projectile = jsonConfig["projectile"].ToObject<Projectile>();
        this.secondary_projectile = (jsonConfig["secondary_projectile"] != null) ? jsonConfig["secondary_projectile"].ToObject<Projectile>() : null;
    }

    public string GetName() {
        return name;
    }

    public int GetManaCost() {
        return RPNEvaluator.RPNEvaluator.Evaluate(this.mana_cost, new Dictionary<string, int>());
    }

    public int GetDamage() {
        return this.damage.amount;
    }

    public float GetCooldown() {
        return RPNEvaluator.RPNEvaluator.Evaluatef(this.cooldown, new Dictionary<string, float>());
    }

    public virtual int GetIcon() {
        return icon;
    }

    public bool IsReady() {
        return (last_cast + GetCooldown() < Time.time);
    }

    public Damage.Type GetDamageType() {
        return damage.type;
    }

    public List<Projectile> GetProjectiles() {
        var result = new List<Projectile> { projectile };
        if (secondary_projectile != null) result.Add(secondary_projectile);
        return result;
    }

    //ICastable requires this implementation so that SpellUI can store ICastables instead
    //I think a better solution would be to see how interfaces require the implementation of properties, but I really don't wanna work on this bug anymore
    public float GetLastCast() {
        return last_cast;
    }

    public IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, string modifierSpeed, Dictionary<string, float> modifierVariables, Action<Hittable, Vector3> OnModifiedHit) {
        this.team = team;
        string speedEquation = (modifierSpeed != null) ? this.projectile.speed + " " + modifierSpeed : this.projectile.speed;
        float speed = RPNEvaluator.RPNEvaluator.Evaluatef(speedEquation, modifierVariables); 
        GameManager.Instance.projectileManager.CreateProjectile(this.icon, this.projectile.trajectory, where, target - where, speed, OnModifiedHit);

        yield return new WaitForEndOfFrame();
    }


    public virtual IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team) {
        this.team = team;
        float speed = RPNEvaluator.RPNEvaluator.Evaluatef(this.projectile.speed, new Dictionary<string, float> { { "power", 1 } });
        int projectileAmount = RPNEvaluator.RPNEvaluator.Evaluate(this.N, new Dictionary<string, int> { { "power", 1 } });
        List<Vector3> targets = new List<Vector3>();
        targets.Add(target);
        if (projectileAmount > 1) {
            float totalAngle = RPNEvaluator.RPNEvaluator.Evaluatef(this.spray, new Dictionary<string, float>());
            float rightBound = -(totalAngle / 2);
            //Debug.Log("totalAngle: " + totalAngle + " | leftBound: " + leftBound + " | rightBound: " + rightBound);
            var rand = new System.Random();
            for (int i = 0; i < projectileAmount; i++) {
                double randomAngle = rightBound + rand.NextDouble() * totalAngle;
                //Debug.Log("randomAngle: " + randomAngle);
                double newTargetX = Math.Cos(randomAngle) * target.x - Math.Sin(randomAngle) * target.y;
                double newTargetY = Math.Sin(randomAngle) * target.x + Math.Cos(randomAngle) * target.y;
                Vector3 newTarget = new Vector3((float)newTargetX, (float)newTargetY, target.z);
                //Debug.Log("target.normalized: " + target.normalized + " | newTarget.normalized: " + newTarget.normalized + " | Vector3.Angle(target, newTarget): " + Vector3.Angle(target, newTarget));
                targets.Add(newTarget);
            }
        }

        foreach (Vector3 listedTarget in targets) {
            GameManager.Instance.projectileManager.CreateProjectile(this.icon, this.projectile.trajectory, where, listedTarget - where, speed, OnHit);
        }

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact) {
        if (other.team != team) {
            other.Damage(this.damage);
            if (team == Hittable.Team.PLAYER) {
                GameManager.Instance.playerStatisticsManager.DamageDealt += GetDamage();
            }
        }

    }
}
