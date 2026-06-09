using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
public class RelicManager
{
	private Dictionary<string, Relic> relics;
	private Dictionary<string, Relic> unownedRelics;
	private static RelicManager theInstance;

	public Dictionary<string, Relic> UnownedRelics {
		get { return unownedRelics; }
	}

	public Relic GetRelic(string name)
	{
		Relic to_give = unownedRelics[name];
		unownedRelics.Remove(name);
		if (unownedRelics.Count == 0)
		{
			foreach(Relic relic in relics.Values)
			{
				unownedRelics.Add(relic.name, relic);
			}
		}
		return to_give;
	}

	public Relic GetRandomRelic()
	{
		System.Random rand = new System.Random();
		Relic to_give = unownedRelics.ElementAt(rand.Next(unownedRelics.Count)).Value;
		unownedRelics.Remove(to_give.name);
		return to_give;
	}

	public Dictionary<string, Relic> GetAllRelics()
	{
		return relics;
	}

	public void AddRelicToUnowned(string name)
	{
		if (!relics.ContainsKey(name))
		{
			Debug.Log("Bad relic name requested in ReturnRelic");
			return;
		}
		unownedRelics.Add(name, relics[name]);
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
		unownedRelics = new Dictionary<string, Relic>();
        JToken parsedRelicsJson = JToken.Parse(Resources.Load<TextAsset>("relics").text);
		foreach (JToken in_relic in parsedRelicsJson) {
			Relic relic = new Relic(in_relic);
			relics.Add(relic.name, relic);
			unownedRelics.Add(relic.name, relic);
		}
    }
}