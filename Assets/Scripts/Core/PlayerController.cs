using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using RPNEvaluator;
using System;

public class PlayerController : MonoBehaviour
{
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUIContainer spellUI;

    public List<Relic> relics;

    public int speed;

    public Unit unit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unit = GetComponent<Unit>();
        GameManager.Instance.player = gameObject;
        EventBus.Instance.OnRelicPickup += OnRelicPickup;
    }

    public void StartLevel()
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());

        spellcaster = new SpellCaster(RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", RPNDict), // max mana
                                      RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", RPNDict), // mana regen
                                      RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", RPNDict), // spell power
                                      Hittable.Team.PLAYER);
        spellUI.ResetSpellUI();
        AddNewSpell(SpellBuilder.Instance.BuildSpell(spellcaster, "arcane_bolt"));
        StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", RPNDict),
                          Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        relics = new List<Relic>();

        // Here's how you give a relic to the player. -Iain
        // EventBus.Instance.TakeRelic(RelicManager.Instance.GetRelic("Jade Elephant"));

        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", RPNDict);

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
    }

    public void StartWave()
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        spellcaster.HandleWaveScaling(RPNEvaluator.RPNEvaluator.Evaluate("90 wave 10 * +", RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate("10 wave +", RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate("wave 10 *", RPNDict));

        hp.SetMaxHP(RPNEvaluator.RPNEvaluator.Evaluate("95 wave 5 * +", RPNDict));

        speed = RPNEvaluator.RPNEvaluator.Evaluate("5", RPNDict);
    }

    public bool AddNewSpell(ICastable spell)
    {
        int index = spellcaster.AddSpell(spell);
        if (index != -1)
        {
            spellUI.spellUIs[index].GetComponent<SpellUI>().SetSpell(spell);
            return true;
        }
        return false;
    }
    public void RemoveSpellAtIndex(int index)
    {
        spellcaster.DropSpell(index);
        spellUI.spellUIs[index].GetComponent<SpellUI>().RemoveSpell();
        spellUI.DeactivateDropButtons();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.state == GameManager.GameState.INWAVE) 
        {
            foreach (Relic r in relics)
            {
                if (r.triggerTimeDelay > 0 && !r.active && r.lastTriggerTime + r.triggerTimeDelay < Time.time)
                {
                    r.Activate();
                }
            }
        }
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
            GameManager.Instance.playerStatisticsManager.DamageFired += spellcaster.spells[spellcaster.current_spell_index].GetDamage();
            GameManager.Instance.playerStatisticsManager.SpellsCasted++;
        }
    }

    void OnMove(InputValue value)
    {
        if (GameManager.Instance.state != GameManager.GameState.COUNTDOWN && GameManager.Instance.state != GameManager.GameState.INWAVE)
        {
            unit.movement = Vector2.zero;
            return;
        }
        unit.movement = value.Get<Vector2>()*speed;
    }

    void OnChangeSpell(InputValue value)
    {
        do {
            spellcaster.current_spell_index++;
            if (spellcaster.current_spell_index >= spellcaster.spells.Length)
            {
                spellcaster.current_spell_index = 0;
            }
        } while (spellcaster.spells[spellcaster.current_spell_index] == null);
    }

    void OnRelicPickup(Relic r)
    {
        relics.Add(r);
        r.BindToOwner(this);
    }

    void Die()
    {
        GameManager.Instance.state = GameManager.GameState.GAMELOST;
    }

}
