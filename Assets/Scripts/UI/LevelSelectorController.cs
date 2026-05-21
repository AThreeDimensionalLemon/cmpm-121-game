using System;
using System.Collections.Generic;
using System.Text;

public class LevelSelectorController : MenuSelectorController {
    public string level;
    public EnemySpawner spawner;

    public override void Setup(string text) {
        level = text;
        label.text = text;
    }

    public override void ExecuteTask() {
        spawner.StartLevel(level);
    }
}