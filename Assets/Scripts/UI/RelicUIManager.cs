using System.Collections.Generic;
using UnityEngine;

public class RelicUIManager : MonoBehaviour
{
    public GameObject relicUIPrefab;
    public PlayerController player;
    private int currRelicIndex;
    private List<GameObject> relicUIList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.Instance.OnRelicPickup += OnRelicPickup;
        currRelicIndex = 0;
        relicUIList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRelicPickup(Relic r)
    {
        // make a new Relic UI representation
        GameObject rui = Instantiate(relicUIPrefab, transform);
        rui.transform.localPosition = new Vector3(-350 + 40 * currRelicIndex, 0, 0);
        RelicUI ruic = rui.GetComponent<RelicUI>();
        ruic.player = player;
        ruic.index = currRelicIndex;
        currRelicIndex++;
        ruic.Instantiate(r);
        relicUIList.Add(rui);
    }

    public void ResetRelicUI()
    {
        foreach(GameObject rui in relicUIList)
        {
            Destroy(rui);
        }
        relicUIList = new List<GameObject>();
        currRelicIndex = 0;
    }
}
