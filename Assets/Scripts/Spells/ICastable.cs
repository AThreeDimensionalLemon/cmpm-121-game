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
    string GetDescription();
    SpellCaster GetOwner();
    int GetManaCost();
    int GetDamage();
    int GetSecondaryDamage();
    void SetSecondaryDamage(int inDamage);
    void SetDamageDicts(Dictionary<string, int> dict);
    float GetCooldown();
    int GetN();
    int GetNumSplits();
    int GetIcon();
    bool IsReady();
    float GetLastCast();
    Damage.Type GetDamageType();
    List<Projectile> GetProjectiles();
    List<Vector3> GetTargetList(Vector3 origin, Vector3 direction, float angleRange, int targetAmount);

    //cast the spell
    IEnumerator Cast(Vector3 where, List<Vector3> target, Hittable.Team team, string modifierSpeed = null, Dictionary<string, float> modifierVariables = null, Action<Hittable, Vector3, int> OnModifiedHit = null, string trajectory = null);
    //IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team);
}
