using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor.PackageManager;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private Level currentLevel = null;

    public void SetLevel(int inLevel) {
        JToken token = JToken.Parse(Resources.Load<TextAsset>("levels").text);
        //currentLevel = token.ToObject(Level[])[inLevel];
    }

    public string GetLevel() {
        if (currentLevel != null) return currentLevel.name;
        else throw new Exception("Current level has not yet been set");
    }

    void Start() {
        GameManager.Instance.levelManager = this;
    }
}
