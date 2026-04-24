using UnityEngine;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting.Antlr3.Runtime;

public class WaveMaker {

    public WaveMaker(int inLevel) {
        
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
