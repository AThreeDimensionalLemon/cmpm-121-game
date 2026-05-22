using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using Newtonsoft.Json.Linq;
using JetBrains.Annotations;

public class PlayerClassesManager : MonoBehaviour {
    JObject parsedClassesJson;
    PlayerClass currentPlayerClass;

    public JObject GetJson() {
        return parsedClassesJson;
    }

    public void SetPlayerClass(string className) {
        string sanitizedClassName = className.ToLower();
        if (parsedClassesJson.ContainsKey(sanitizedClassName) == false) throw new ArgumentException("could not find the \"" + className + "\" class in classes JSON");
        currentPlayerClass =  parsedClassesJson[sanitizedClassName].ToObject<PlayerClass>();
    }

    public PlayerClass GetPlayerClass() {
        if (currentPlayerClass == null) throw new Exception("Player class is not set");
        return currentPlayerClass;
    }

    void Start() {
        GameManager.Instance.playerClassesManager = this;
        parsedClassesJson = JObject.Parse(Resources.Load<TextAsset>("classes").text);
    }
}
