using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

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
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("power", owner.spell_power);
        this.damage = new Damage(jsonConfig["damage"], RPNDict);
        this.secondary_damage = (jsonConfig["secondary_damage"] != null) ? jsonConfig["secondary_damage"].ToString() : "0";
        this.mana_cost = jsonConfig["mana_cost"].ToString();
        this.cooldown = jsonConfig["cooldown"].ToString();
        this.projectile = jsonConfig["projectile"].ToObject<Projectile>();
        this.secondary_projectile = (jsonConfig["secondary_projectile"] != null) ? jsonConfig["secondary_projectile"].ToObject<Projectile>() : null;
    }

    public string GetName() {
        return name;
    }

    public SpellCaster GetOwner()
    {
        return owner;
    }

    public int GetManaCost() {
        return RPNEvaluator.RPNEvaluator.Evaluate(this.mana_cost, new Dictionary<string, int> { { "power", 1 } });
    }

    public int GetDamage() {
        return RPNEvaluator.RPNEvaluator.Evaluate(this.damage.amount, this.damage.damage_dict);
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
    public float GetLastCast() {
        return last_cast;
    }

    private List<Vector3> GetTargetList(Vector3 origin, Vector3 direction, float angleRange, int targetAmount) {
        List<Vector3> result = new();
        System.Random rand = new();
        Vector2 initLocDir = Vector2.Normalize(direction - origin);
        for (int i = 0; i < targetAmount; i++) {
            double randAng = -(angleRange / 2) + rand.NextDouble() * (angleRange);
            double newLocDirX = Math.Cos(randAng) * initLocDir.x - Math.Sin(randAng) * initLocDir.y;
            double newLocDirY = Math.Sin(randAng) * initLocDir.x + Math.Cos(randAng) * initLocDir.y;
            Vector3 newWorDir = new((float)newLocDirX + origin.x, (float)newLocDirY + origin.y, direction.z);
            result.Add(newWorDir);
        }
        return result;
    }

    public IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, string modifierSpeed = null, Dictionary<string, float> modifierVariables = null, Action<Hittable, Vector3> InHitEvent = null) {
        this.team = team;
        Dictionary<string, int> RPNDictInt = new Dictionary<string, int>();
        Dictionary<string, float> RPNDictFloat = new Dictionary<string, float>();
        RPNDictInt.Add("power", owner.spell_power);
        RPNDictFloat.Add("power", owner.spell_power);

        //set default variables here because none of them are compile-time constants
        string speedEquation = (modifierSpeed != null) ? this.projectile.speed + " " + modifierSpeed : this.projectile.speed;
        var variables = (modifierVariables != null) ? modifierVariables : RPNDictFloat;
        float speed = RPNEvaluator.RPNEvaluator.Evaluatef(speedEquation, variables);
        Action<Hittable, Vector3> HitEvent = (InHitEvent != null) ? InHitEvent : OnHit;

        //prepare multiple projectiles, if applicable
        int intN = RPNEvaluator.RPNEvaluator.Evaluate(this.N, RPNDictInt);
        List<Vector3> targets = new() { target };
        if (intN > 1 && secondary_projectile == null) {
            float angle = RPNEvaluator.RPNEvaluator.Evaluatef(this.spray, new Dictionary<string, float>());
            targets.AddRange(GetTargetList(where, target, angle, intN));
        }

        //spawn projectiles
        foreach (Vector3 listedTarget in targets) {
            GameManager.Instance.projectileManager.CreateProjectile(this.icon, this.projectile.trajectory, where, listedTarget - where, speed, HitEvent);
        }

        yield return new WaitForEndOfFrame();
    }

    void OnHit(Hittable other, Vector3 impact) {
        if (other.team != team) {
            other.Damage(this.damage);
            if (this.projectile.is_splitting) {
                Debug.Log("split!");
                int intN = RPNEvaluator.RPNEvaluator.Evaluate(this.N, new Dictionary<string, int> { { "power", 1 } });
                float speed = RPNEvaluator.RPNEvaluator.Evaluatef(secondary_projectile.speed, new Dictionary<string, float> { { "power", 1 } });
                float lifetime = RPNEvaluator.RPNEvaluator.Evaluatef(secondary_projectile.lifetime, new Dictionary<string, float> { { "power", 1 } });
                foreach (Vector3 target in GetTargetList(impact, Vector3.right, 2 * (float)Math.PI, intN)) {
                    GameManager.Instance.projectileManager.CreateProjectile(this.icon, this.secondary_projectile.trajectory, impact, target - impact, speed, OnHit, lifetime, other);
                }
            }
            if (team == Hittable.Team.PLAYER) {
                GameManager.Instance.playerStatisticsManager.DamageDealt += GetDamage();
            }
        }
    }
}
