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
