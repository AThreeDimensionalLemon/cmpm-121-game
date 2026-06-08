using System;
using System.Collections.Generic;
using System.Text;

class TreeSelectorController : MenuSelectorController {
    public override void Setup(string text) {
        label.text = text;
    }

    public override void ExecuteTask() {

    }
}
