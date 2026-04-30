using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerStatisticsManager : MonoBehaviour
{
    public int DamageFired;
    public int DamageDealt;
    public int SpellsCasted;
    public float TimeElapsed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.playerStatisticsManager = this;
    }

    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            TimeElapsed += Time.deltaTime;
        }
    }

    public void ResetStatistics()
    {
        DamageFired = 0;
        DamageDealt = 0;
        SpellsCasted = 0;
        TimeElapsed = 0.0f;
    }

    public String GetStatisticsReadout()
    {
        int efficiency = (int)(((float)DamageDealt / DamageFired) * 100);
        string text = "Time elapsed: " + (int)TimeElapsed + " seconds"
                       + "\nDamage Dealt: " + DamageDealt
                       + "\nDamage Efficiency: " + efficiency + '%'
                       + "\nSpells Cast: " + SpellsCasted;
        return text;

    }

}
