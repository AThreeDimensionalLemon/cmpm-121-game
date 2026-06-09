using System;
using System.Collections.Generic;
using System.Text;

public class LevelSelectorController : MenuSelectorController {
    public string level;

    public void Setup(string text) {
        level = text;
        label.text = text;
    }

    public void ExecuteTask() {
        spawner.StartClassChoosing(level);
    }
}