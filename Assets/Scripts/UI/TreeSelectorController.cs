using System;
using System.Collections.Generic;
using System.Text;

class TreeSelectorController : MenuSelectorController {

    public void Setup(string text) {
        label.text = text;
    }

    public void ExecuteTask() {
        TreeSpellAndRelicAdapter.Instance.ApplyReward(label.text);
    }
}
