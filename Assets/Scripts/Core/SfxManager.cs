using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SfxManager : MonoBehaviour {
    [SerializeField] GameObject player;
    Action<SpellCaster> OnSpellCast;

    void Start() {
        EventBus eventBus = EventBus.Instance;

        OnSpellCast = (caster) => {
            player.GetComponent<AudioSource>().Play();
        };
        eventBus.OnSpellCast += OnSpellCast;
    }
}
