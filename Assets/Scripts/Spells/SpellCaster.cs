using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team team;
    public ICastable spell;
    public int spell_power;

    public IEnumerator ManaRegeneration()
    {
        while (true)
        {
            mana += mana_reg;
            mana = Mathf.Min(mana, max_mana);
            yield return new WaitForSeconds(1);
        }
    }

    public SpellCaster(int mana, int mana_reg, int spell_power, Hittable.Team team)
    {
        this.mana = mana;
        this.max_mana = mana;
        this.mana_reg = mana_reg;
        this.spell_power = spell_power;
        this.team = team;
        spell = SpellBuilder.Instance.BuildSpell(this, "arcane_bolt");
        spell = SpellBuilder.Instance.ModifySpell(spell, "damage_amp");
        //Debug.Log(spell.GetName());
    }

    public void HandleWaveScaling(int mana, int mana_reg, int spell_power)
    {
        this.mana = mana;
        this.mana_reg = mana_reg;
        this.spell_power = spell_power;
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {        
        if (mana >= spell.GetManaCost() && spell.IsReady())
        {
            mana -= spell.GetManaCost();
            Debug.Log(spell.GetName() + " spent " + spell.GetManaCost() + " mana to deal " + spell.GetDamage() + " damage");
            yield return spell.Cast(where, target, team);
        }
        yield break;
    }

}
