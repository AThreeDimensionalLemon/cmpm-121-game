using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameManager 
{
    public enum GameState
    {
        PREGAME,
        INWAVE,
        WAVEEND,
        COUNTDOWN,
        GAMEOVER,
        GAMELOST
    }
    public GameState state;

    public int countdown;
    private static GameManager theInstance;
    public static GameManager Instance {  get
        {
            if (theInstance == null)
                theInstance = new GameManager();
            return theInstance;
        }
    }

    private int currentWave = 1;

    public void ResetWaves()
    {
        currentWave = 1;
    }

    public void KillAllEnemies()
    {
        while (enemies.Count > 0)
        {
            GameObject enemy = enemies[0];
            Hittable hp = enemy.GetComponent<EnemyController>().hp;
            hp.Damage(new Damage(hp.hp, Damage.Type.PHYSICAL));
        }
    }

    public void IncrementWave()
    {
        currentWave++;
        if (currentWave > levelManager.GetLevel().waves)
        {
            theInstance.state = GameState.GAMEOVER;
        }
    }

    public int GetWave()
    {
        return currentWave;
    }

    public GameObject player;
    
    public ProjectileManager projectileManager;
    public SpellIconManager spellIconManager;
    public EnemySpriteManager enemySpriteManager;
    public PlayerSpriteManager playerSpriteManager;
    public RelicIconManager relicIconManager;
    public LevelManager levelManager;

    private List<GameObject> enemies;
    public int enemy_count { get { return enemies.Count; } }

    public void AddEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }

    public GameObject GetClosestEnemy(Vector3 point)
    {
        if (enemies == null || enemies.Count == 0) return null;
        if (enemies.Count == 1) return enemies[0];
        return enemies.Aggregate((a,b) => (a.transform.position - point).sqrMagnitude < (b.transform.position - point).sqrMagnitude ? a : b);
    }

    private GameManager()
    {
        enemies = new List<GameObject>();
    }
}
