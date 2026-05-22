using UnityEngine;
using TMPro;

public abstract class MenuSelectorController : MonoBehaviour {
    public TextMeshProUGUI label;

    public abstract void Setup(string text);
    public abstract void ExecuteTask();
}