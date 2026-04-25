using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private static Level currentLevel = null;

    public void SetLevel(string inLevel) {
        JToken levelTokens = JToken.Parse(Resources.Load<TextAsset>("levels").text);
        foreach (var token in levelTokens) {
            if (token.Value<string>("name") == inLevel) {
                currentLevel = token.ToObject<Level>();
                break;
            }
        }
    }

    public Level GetLevel() {
        if (currentLevel != null) return currentLevel;
        else throw new Exception("Current level has not yet been set");
    }

    void Start() {
        GameManager.Instance.levelManager = this;
    }
}
