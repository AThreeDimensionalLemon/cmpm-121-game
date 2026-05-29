using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using RPNEvaluator;
using System;
using NUnit.Framework;

public class PlayerController : MonoBehaviour
{
    public PlayerClass playerClass;
    public SpriteRenderer spriteRenderer;
    public Hittable hp;
    public HealthBar healthui;
    public ManaBar manaui;

    public SpellCaster spellcaster;
    public SpellUIContainer spellUI;

    public List<Relic> relics;

    private int Speed;
    public int speed { get { return EventBus.Instance.GetSpeed(Speed); } set { Speed = value; } }

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
        this.playerClass = GameManager.Instance.playerClassesManager.GetPlayerClass();
        spriteRenderer.sprite = GameManager.Instance.playerSpriteManager.Get(this.playerClass.sprite);
        Debug.Assert(playerClass != null, "Player does not have a class");

        spellcaster = new SpellCaster(RPNEvaluator.RPNEvaluator.Evaluate(playerClass.mana, RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate(playerClass.mana_regeneration, RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate(playerClass.spellpower, RPNDict),
                                      Hittable.Team.PLAYER);
        spellUI.ResetSpellUI();
        AddNewSpell(SpellBuilder.Instance.BuildSpell(spellcaster, "arcane_bolt"));
        StartCoroutine(spellcaster.ManaRegeneration());

        hp = new Hittable(RPNEvaluator.RPNEvaluator.Evaluate(playerClass.health, RPNDict),
                          Hittable.Team.PLAYER, gameObject);
        hp.OnDeath += Die;
        hp.team = Hittable.Team.PLAYER;

        relics = new List<Relic>();

        // Here's how you give a relic to the player. -Iain
        // EventBus.Instance.TakeRelic(RelicManager.Instance.GetRelic("Green Gem"));

        speed = RPNEvaluator.RPNEvaluator.Evaluate(playerClass.speed, RPNDict);

        // tell UI elements what to show
        healthui.SetHealth(hp);
        manaui.SetSpellCaster(spellcaster);
    }

    public void StartWave()
    {
        Dictionary<string, int> RPNDict = new Dictionary<string, int>();
        RPNDict.Add("wave", GameManager.Instance.GetWave());
        spellcaster.HandleWaveScaling(RPNEvaluator.RPNEvaluator.Evaluate(playerClass.mana, RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate(playerClass.mana_regeneration, RPNDict),
                                      RPNEvaluator.RPNEvaluator.Evaluate(playerClass.spellpower, RPNDict));

        hp.SetMaxHP(RPNEvaluator.RPNEvaluator.Evaluate(playerClass.health, RPNDict));

        speed = RPNEvaluator.RPNEvaluator.Evaluate(playerClass.speed, RPNDict);
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
                if (r.trigger.type == Relic.TriggerType.stand_still && !r.active && r.lastTriggerTime + r.triggerTimeDelay < Time.time)
                {
                    r.Activate();
                }
                if (r.effect.until == "time-passed" && r.active && r.lastTriggerTime + r.untilTimeDelay < Time.time)
                {
                    r.Deactivate();
                }

                if (r.trigger.type == Relic.TriggerType.max_health  && !r.active && hp.hp == hp.max_hp)
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
