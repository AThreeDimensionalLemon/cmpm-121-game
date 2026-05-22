using UnityEngine;
using TMPro;

public abstract class MenuSelectorController : MonoBehaviour {
    public TextMeshProUGUI label;
    public EnemySpawner spawner;

    public abstract void Setup(string text);
    public abstract void ExecuteTask();
}