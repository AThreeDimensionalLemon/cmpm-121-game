using Newtonsoft.Json.Linq;
using RPNEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public interface ICastable {
    string GetName();
    int GetManaCost();
    int GetDamage();
    float GetCooldown();
    int GetIcon();
    bool IsReady();
    float GetLastCast();
    IEnumerator Cast(Vector3 where, Vector3 target, Hittable.Team team);
}
