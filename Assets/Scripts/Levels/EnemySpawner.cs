using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using RPNEvaluator;

public class EnemySpawner : MonoBehaviour
{
    private string EnemiesJsonPath = "enemies";

    public Image level_selector; //background of level selection window
    public GameObject button; //prefab of buttons
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;
    public Dictionary<string, Enemy> enemy_prototypes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() { //instantiate buttons for level selection
        float windowHeight = level_selector.GetComponent<RectTransform>().rect.height;
        float windowBorderSize = button.GetComponent<RectTransform>().offsetMin.x; //how far in the window's borders extend in
        float buttonYBuffer = 4;
        JArray levelsJson = GameManager.Instance.levelManager.GetJson();
        int levelsCount = levelsJson.Count();

        for (int i = 0; i < levelsCount; ++i) {
            GameObject selector = Instantiate(button, level_selector.transform);
            RectTransform selectorDims = selector.GetComponent<RectTransform>();
            float windowSafeAreaHeight = (windowHeight - windowBorderSize * 2);

            selectorDims.sizeDelta = new Vector2(selectorDims.sizeDelta.x, windowSafeAreaHeight / levelsCount - buttonYBuffer * 2);
            float selectorHeight = selectorDims.rect.height;
            selector.transform.localPosition = new Vector3(0, windowHeight / 2 - (windowBorderSize + (selectorHeight + buttonYBuffer * 2) * i));

            selector.GetComponent<MenuSelectorController>().spawner = this;
            selector.GetComponent<MenuSelectorController>().SetLevel(levelsJson[i]["name"].ToObject<string>());
        }

        enemy_prototypes = new Dictionary<string, Enemy>();
        JToken json = JToken.Parse(Resources.Load<TextAsset>(EnemiesJsonPath).text);
        foreach(JToken token in json)
        {
            Enemy in_enemy = token.ToObject<Enemy>();
            enemy_prototypes.Add(in_enemy.name, in_enemy);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel(string levelname)
    {
        GameManager manager = GameManager.Instance;
        level_selector.gameObject.SetActive(false);
        // this is not nice: we should not have to be required to tell the player directly that the level is starting
        manager.player.GetComponent<PlayerController>().StartLevel();

        manager.levelManager.SetLevel(levelname);
        manager.ResetWaves();

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        GameManager.Instance.IncrementWave();
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager manager = GameManager.Instance;
        manager.state = GameManager.GameState.COUNTDOWN;
        manager.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            manager.countdown--;
        }
        manager.state = GameManager.GameState.INWAVE;

        Level currLevel = manager.levelManager.GetLevel();
        int wave = manager.GetWave();
        foreach(Spawn spawn in manager.levelManager.GetLevel().spawns)
        {
            Enemy to_spawn = new Enemy(enemy_prototypes[spawn.enemy]);
            yield return SpawnEnemy(FindValidSpawnPoint(spawn.location), to_spawn, spawn.delay);
        }
        yield return new WaitWhile(() => manager.enemy_count > 0);
        manager.state = GameManager.GameState.WAVEEND;
    }

    SpawnPoint FindValidSpawnPoint(string in_behavior)
    {
        string[] tokens = in_behavior.Split(' ');
        if (tokens.Length == 1)
        {
            return SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        }
        else
        {
            List<SpawnPoint> curated_spawns = new List<SpawnPoint>();
            foreach (SpawnPoint point in SpawnPoints)
            {
                if (point.kind.ToString().ToLower().Equals(tokens[1].ToLower())) {
                    curated_spawns.Add(point);
                }
            }
            return curated_spawns[Random.Range(0, curated_spawns.Count)];
        }
    }

    IEnumerator SpawnZombie()
    {
        SpawnPoint spawn_point = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
        Vector2 offset = Random.insideUnitCircle * 1.8f;
                
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(0);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(50, Hittable.Team.MONSTERS, new_enemy);
        en.speed = 10;
        en.damage = 5;
        en.damage_type = Damage.Type.PHYSICAL;
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator SpawnEnemy(SpawnPoint spawn_point, Enemy in_enemy, int in_delay)
    {
        Vector2 offset = Random.insideUnitCircle * 1.8f;
        Vector3 initial_position = spawn_point.transform.position + new Vector3(offset.x, offset.y, 0);
        GameObject new_enemy = Instantiate(enemy, initial_position, Quaternion.identity);

        new_enemy.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.enemySpriteManager.Get(in_enemy.sprite);
        EnemyController en = new_enemy.GetComponent<EnemyController>();
        en.hp = new Hittable(in_enemy.hp, Hittable.Team.MONSTERS, new_enemy);
        en.speed = in_enemy.speed;
        en.damage = in_enemy.damage;
        en.damage_type = Damage.Type.PHYSICAL; // leaving this hardcoded for now, because we haven't added enemy damage types into the JSON yet
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(in_delay);
    }
}
