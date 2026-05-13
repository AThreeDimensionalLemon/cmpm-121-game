using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RPNEvaluator;

public class Projectile {
    public string trajectory;
    public string speed;
    public string lifetime = "0";
    public int sprite;
    public bool is_splitting = false;
}
