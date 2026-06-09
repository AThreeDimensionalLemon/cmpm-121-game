using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ClassSelectorController : MenuSelectorController {
    public string playerClass;
    public PlayerController playerController;

    public void Setup(string text) {
        playerClass = text;
        label.text = text;
    }

    public void ExecuteTask() {
        gameObject.transform.parent.GetComponent<AudioSource>().Play();
        GameManager.Instance.playerClassesManager.SetPlayerClass(playerClass);
        spawner.StartLevel();
    }
}