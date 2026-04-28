using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;

public class EnemySpawner : MonoBehaviour
{
    public Image level_selector; //background of level selection window
    public GameObject button; //prefab of buttons
    public GameObject enemy;
    public SpawnPoint[] SpawnPoints;    

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
        manager.levelManager.SetLevel("Easy"); //TODO: Make buttons that send the level names

        StartCoroutine(SpawnWave());
    }

    public void NextWave()
    {
        StartCoroutine(SpawnWave());
    }


    IEnumerator SpawnWave()
    {
        GameManager.Instance.state = GameManager.GameState.COUNTDOWN;
        GameManager.Instance.countdown = 3;
        for (int i = 3; i > 0; i--)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.countdown--;
        }
        GameManager.Instance.state = GameManager.GameState.INWAVE;
        for (int i = 0; i < 10; ++i)
        {
            yield return SpawnZombie();
        }
        yield return new WaitWhile(() => GameManager.Instance.enemy_count > 0);
        GameManager.Instance.state = GameManager.GameState.WAVEEND;
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
        GameManager.Instance.AddEnemy(new_enemy);
        yield return new WaitForSeconds(0.5f);
    }
}
