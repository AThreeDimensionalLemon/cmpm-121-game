using UnityEngine;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;

public class WaveFactory {
    private Level loadedLevel;

    public WaveFactory(int inLevel) {
        JToken token = JToken.Parse(Resources.Load<TextAsset>("levels").text);
        loadedLevel = token.ToObject(Level);
    }

    public Wave Make(int wave) {
        return new Wave();
    }

    public string LoadedLevel {
        get {
            return loadedLevel.name;
        }
    }
}
