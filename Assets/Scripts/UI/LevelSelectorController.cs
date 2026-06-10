using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class LevelSelectorController : MenuSelectorController {
    public string level;

    public void Setup(string text) {
        level = text;
        label.text = text;
    }

    public void ExecuteTask() {
        AudioSource audioSource = gameObject.transform.parent.GetComponent<AudioSource>();
        audioSource.Play();
        spawner.StartClassChoosing(level);
    }
}