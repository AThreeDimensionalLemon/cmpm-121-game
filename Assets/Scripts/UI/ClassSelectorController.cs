using System;
using System.Collections.Generic;
using System.Text;

public class ClassSelectorController : MenuSelectorController {
    public string playerClass;
    public PlayerController playerController;

    public override void Setup(string text) {
        playerClass = text;
        label.text = text;
    }

    public override void ExecuteTask() {
        GameManager.Instance.playerClassesManager.SetPlayerClass(playerClass); //maybe I should get rid of this boilerplate after all?
        playerController.playerClass = GameManager.Instance.playerClassesManager.GetPlayerClass();
    }
}