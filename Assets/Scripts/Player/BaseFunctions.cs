using UnityEngine;
using TMPro;
using System.IO;

public class BaseFunctions : MonoBehaviour
{
    [SerializeField] private GameObject extraMenu;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private TMP_Text warningText;

    private bool isActive = false;


    public void ChangeActive()
    {
        extraMenu.SetActive(!isActive);
        isActive = !isActive;
        if (isActive) {
            buttonText.text = "B A C K";
        }
        else {
            buttonText.text = "E X T R A  M E N U!";
        }
    }

    private string GetSavePath()
    {
        return Application.persistentDataPath + "/save.json";
    }

    public void DeleteGame() {
        if (File.Exists(GetSavePath()))
        {
            File.Delete(GetSavePath());
            warningText.text = "Game DELETED!";
            return;
        }
        warningText.text = "You doesnt have saves!";
    }
}
