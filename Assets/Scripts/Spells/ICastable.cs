using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public interface ICastable {
    string GetName();
    int GetManaCost();
    int GetDamage();
    float GetCooldown();
    int GetIcon();
    bool IsReady();
    IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team);
}
