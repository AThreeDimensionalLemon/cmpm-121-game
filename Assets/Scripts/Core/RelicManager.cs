using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
public class RelicManager
{
	private Dictionary<string, Relic> relics;
	private static RelicManager theInstance;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public static RelicManager Instance {
		get {
			if (theInstance == null) theInstance = new RelicManager();
			return theInstance;
		}
    }

    private RelicManager()
    {
		relics = new Dictionary<string, Relic>();
        JToken parsedRelicsJson = JToken.Parse(Resources.Load<TextAsset>("relics").text);
		foreach (JToken in_relic in parsedRelicsJson) {
			Relic relic = new Relic(in_relic);
			relics.Add(relic.name, relic);
		}
		foreach(string relic in relics.Keys)
		{
			Debug.Log(relic);
			Debug.Log(relics[relic].ToString());
		}
    }
}