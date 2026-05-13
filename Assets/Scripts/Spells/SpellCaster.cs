using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpellCaster 
{
    public int mana;
    public int max_mana;
    public int mana_reg;
    public Hittable.Team team;
    public ICastable[] spells;
    public int current_spell_index;
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
        this.spells = new ICastable[4];
        for (int i = 0; i < spells.Length; i++)
        {
            spells[i] = null;
        }
        current_spell_index = 0;
        //spell = SpellBuilder.Instance.ModifySpell(spell, "damage_amp");
        //Debug.Log(spell.GetName());
    }

    public bool IsFull()
    {
        for (int i = 0; i < spells.Length; i++)
        {
            if (spells[i] == null)
            {
                return false;
            }
        }
        return true;
    }

    public int AddSpell(ICastable spell)
    {
        bool added = false;
        int i;
        for (i = 0; i < spells.Length; i++)
        {
            if (spells[i] == null)
            {
                spells[i] = spell;
                current_spell_index = i;
                added = true;
                break;
            }
        }
        if (!added)
        {
            Debug.Log("Can't add spell; spellbook is full!");
            return -1;
        }
        return i;
    }

    public void DropSpell(int index)
    {
        spells[index] = null;
        if (current_spell_index == index)
        {
            if (index == 0)
            {
                current_spell_index = 1;
            }
            else
            {
                current_spell_index = 0;
            }
        }
    }

    public void HandleWaveScaling(int mana, int mana_reg, int spell_power)
    {
        this.mana = mana;
        this.mana_reg = mana_reg;
        this.spell_power = spell_power;
    }

    public IEnumerator Cast(Vector3 where, Vector3 target)
    {
        ICastable current_spell = spells[current_spell_index];
        if (mana >= current_spell.GetManaCost() && current_spell.IsReady())
        {
            mana -= current_spell.GetManaCost();
            Debug.Log(spells[current_spell_index].GetName() + " spent " + current_spell.GetManaCost() + " mana to deal " + current_spell.GetDamage() + " damage");
            yield return current_spell.Cast(where, new List<Vector3> { target }, team);
        }
        yield break;
    }

}
