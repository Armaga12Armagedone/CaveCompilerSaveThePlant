using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resourceText;

    void Start() {
        resourceText.text = "Empty Resources";
    }

    public void UpdateText(Dictionary<ItemData, int> items) {
        string text = "";

        foreach (var item in items) {
            text += item.Key.displayName + ": " + item.Value.ToString();
        }

        resourceText.text = text;
    }
}
