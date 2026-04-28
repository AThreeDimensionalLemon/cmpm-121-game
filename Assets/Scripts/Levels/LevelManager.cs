using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private Level currentLevel = null;
    private JArray parsedLevelJson;

    public void SetLevel(string inLevel) {
        foreach (var token in parsedLevelJson) {
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

    public JArray GetJson() {
        return parsedLevelJson;
    }

    void Start() {
        GameManager.Instance.levelManager = this;
        parsedLevelJson = JArray.Parse(Resources.Load<TextAsset>("levels").text);
    }
}
