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
using Unity.VisualScripting;

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

    // I don't like that this is here, but I don't know how to make the
    // restart game button talk directly to the GameManager because the
    // GameManager isn't a MonoBehavior. So, it's here. I know it shouldn't
    // be. -Iain
    public void RestartGame()
    {
        GameManager.Instance.state = GameManager.GameState.PREGAME;
        GameManager.Instance.ResetWaves();
        GameManager.Instance.playerStatisticsManager.ResetStatistics();
        level_selector.GameObject().SetActive(true);
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
        if (GameManager.Instance.state != GameManager.GameState.GAMEOVER)
        {
            GameManager.Instance.player.GetComponent<PlayerController>().StartWave();
            StartCoroutine(SpawnWave());
        }
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
        EventBus.Instance.DoWaveStart(GameManager.Instance.GetWave());

        Level currLevel = manager.levelManager.GetLevel();
        int wave = manager.GetWave();
        foreach(Spawn spawn in manager.levelManager.GetLevel().spawns)
        {
            StartCoroutine(SpawnWaveSegment(spawn));
        }
        yield return CalculateWaveLength(manager.levelManager.GetLevel().spawns);
        yield return new WaitWhile(() => manager.enemy_count > 0);
        GameManager.Instance.IncrementWave();
        if (GameManager.Instance.state != GameManager.GameState.GAMEOVER && GameManager.Instance.state != GameManager.GameState.GAMELOST)
        {
            manager.state = GameManager.GameState.WAVEEND;
            EventBus.Instance.DoWaveEnd(GameManager.Instance.GetWave() - 1);
        }
    }

    // spawns all enemies of the type given by in_spawn.
    // called asynchronously by SpawnWave().
    IEnumerator SpawnWaveSegment(Spawn in_spawn)
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        int enemies_to_spawn = RPNEvaluator.RPNEvaluator.Evaluate(in_spawn.count, RPNDict);
        int sequence_index = 0;
        Enemy to_spawn = new Enemy(enemy_prototypes[in_spawn.enemy]);

        // initialize enemy prototype stats
        RPNDict.Add("base", enemy_prototypes[in_spawn.enemy].hp);
        to_spawn.hp = RPNEvaluator.RPNEvaluator.Evaluate(in_spawn.hp, RPNDict);
        RPNDict["base"] = enemy_prototypes[in_spawn.enemy].speed;
        to_spawn.speed = RPNEvaluator.RPNEvaluator.Evaluate(in_spawn.speed, RPNDict);
        RPNDict["base"] = enemy_prototypes[in_spawn.enemy].damage;
        to_spawn.damage = RPNEvaluator.RPNEvaluator.Evaluate(in_spawn.damage, RPNDict);

        while(enemies_to_spawn > 0)
        {
            for(int i = 0; i < in_spawn.sequence[sequence_index] - 1; i++) {
                if (enemies_to_spawn > 0)
                {
                    yield return SpawnEnemy(FindValidSpawnPoint(in_spawn.location), to_spawn, 0);
                }
                enemies_to_spawn--;
            }
            if (enemies_to_spawn > 0)
            {
                yield return SpawnEnemy(FindValidSpawnPoint(in_spawn.location), to_spawn, RPNEvaluator.RPNEvaluator.Evaluate(in_spawn.delay, RPNDict));
            }
            enemies_to_spawn--;
            sequence_index++;
            if (sequence_index >= in_spawn.sequence.Length)
            {
                sequence_index = 0;
            }
        }
    }

    // determines how long the longest SpawnWaveSegment() will take to run
    // for the current wave, and returns a WaitForSeconds with that length.
    IEnumerator CalculateWaveLength(Spawn[] in_spawns)
    {
        int wave_length = 0;
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        foreach(Spawn spawn in in_spawns)
        {
            int delay = RPNEvaluator.RPNEvaluator.Evaluate(spawn.delay, RPNDict);
            int count = RPNEvaluator.RPNEvaluator.Evaluate(spawn.count, RPNDict);
            int spawn_length = 0;
            int sequence_index = 0;
            while (count > 0)
            {
                count -= spawn.sequence[sequence_index];
                spawn_length += delay;
                sequence_index++;
                if(sequence_index >= spawn.sequence.Length)
                {
                    sequence_index = 0;
                }
            }
            if (spawn_length > wave_length)
            {
                wave_length = spawn_length;
            }
        }
        yield return new WaitForSeconds(wave_length);
    }

    // Given the 'location' field from a Spawn, finds and
    // returns a valid spawn point for that enemy.
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
