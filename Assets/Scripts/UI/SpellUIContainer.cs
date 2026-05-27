using UnityEngine;

public class SpellUIContainer : MonoBehaviour
{
    public GameObject[] spellUIs;
    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // we only have one spell (right now)
        spellUIs[0].SetActive(true);
        for(int i = 1; i< spellUIs.Length; ++i)
        {
            spellUIs[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // this absolutely should be a listener and not this loop in update -- lynn
        if (GameManager.Instance.state != GameManager.GameState.PREGAME) {
            for (int i = 0; i < 4; i++) {
                spellUIs[i].GetComponent<SpellUI>().highlight.SetActive(player.spellcaster.current_spell_index == i);
            }
        }
    }

    public void ResetSpellUI()
    {
        foreach (GameObject spellui in spellUIs)
        {
            spellui.GetComponent<SpellUI>().RemoveSpell();
            spellui.SetActive(false);
        }
    }

    public void ActivateDropButtons()
    {
        foreach (GameObject spellui in spellUIs)
        {
            if (spellui.activeSelf)
            {
                spellui.GetComponent<SpellUI>().dropbutton.SetActive(true);
            }
        }
    }

    public void DeactivateDropButtons()
    {
        foreach (GameObject spellui in spellUIs)
        {
            if (spellui.activeSelf)
            {
                spellui.GetComponent<SpellUI>().dropbutton.SetActive(false);
            }
        }
    }

}
