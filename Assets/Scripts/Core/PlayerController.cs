using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using RPNEvaluator;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUI spellui;

    public int speed;

    public Unit unit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
    }

    public void StartLevel()
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());

        spellcaster = new SpellCaster(RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", RPNDict),
                                      /*RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", RPNDict),*/
                                      Hittable.Team.PLAYER);
        StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", RPNDict),
                          Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", RPNDict);

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
        spellui.SetSpell(spellcaster.spell);
    }

    public void StartWave()
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());

        spellcaster.max_mana = RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", RPNDict);
        spellcaster.mana_reg = RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", RPNDict);
        //spellcaster.spell_power = RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", RPNDict);

        hp.SetMaxHP(RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", RPNDict));

        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", RPNDict);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnAttack(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER || GameManager.Instance.state == GameManager.GameState.GAMELOST) return;
        Vector2 mouseScreen = Mouse.current.position.value;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0;
        StartCoroutine(spellcaster.Cast(transform.position, mouseWorld));
        if (GameManager.Instance.state == GameManager.GameState.INWAVE)
        {
            GameManager.Instance.playerStatisticsManager.DamageFired += spellcaster.spell.GetDamage();
            GameManager.Instance.playerStatisticsManager.SpellsCasted++;
        }
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state == GameManager.GameState.PREGAME || GameManager.Instance.state == GameManager.GameState.GAMEOVER) return;
        unit.movement = value.Get<Vector2>()*speed;
    }

    void Die()
    {
        GameManager.Instance.state = GameManager.GameState.GAMELOST;
    }

}
