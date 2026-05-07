using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using RPNEvaluator;

public class Projectile {
    private ProjectileMovement trajectory;
    private int sprite;

    public Projectile(JToken projectileToken) {

        var childTokens = projectileToken.ToObject<Dictionary<string, string>>();

        var rpnEvalDict = new Dictionary<string, float> {

        };
        float speed = RPNEvaluator.RPNEvaluator.Evaluatef(childTokens["speed"], rpnEvalDict);
        float lifetime = (childTokens.ContainsKey("lifetime")) ? RPNEvaluator.RPNEvaluator.Evaluatef(childTokens["lifetime"], rpnEvalDict) : -1;

        trajectory = (childTokens["trajectory"] == "straight") ? new StraightProjectileMovement(speed) :
            (childTokens["trajectory"] == "homing") ? new HomingProjectileMovement(speed) :
            new SpiralingProjectileMovement(speed);
        sprite = projectileToken["sprite"].ToObject<int>();
    }
}
