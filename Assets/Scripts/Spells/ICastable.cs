using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public interface ICastable {

    //stuff that probably could've been implemented as properties
    string GetName();
    SpellCaster GetOwner();
    int GetManaCost();
    int GetDamage();
    float GetCooldown();
    int GetIcon();
    bool IsReady();
    float GetLastCast();
    Damage.Type GetDamageType();
    List<Projectile> GetProjectiles();

    //cast the spell
    IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team, string modifierSpeed = null, Dictionary<string, float> modifierVariables = null, Action<Hittable, Vector3> OnModifiedHit = null);
    //IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team);
}
