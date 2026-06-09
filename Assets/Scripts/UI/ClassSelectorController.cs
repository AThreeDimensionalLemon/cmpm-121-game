using System;
using System.Collections.Generic;
using System.Text;

public class ClassSelectorController : MenuSelectorController {
    public string playerClass;
    public PlayerController playerController;

    public void Setup(string text) {
        playerClass = text;
        label.text = text;
    }

    public void ExecuteTask() {
        GameManager.Instance.playerClassesManager.SetPlayerClass(playerClass);
        spawner.StartLevel();
    }
}