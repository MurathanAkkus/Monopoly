
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Serializable]
    public class PlayerSelect
    {
        public TMP_InputField nameInput;
        public TMP_Dropdown typeDropDown;
        public TMP_Dropdown colorDropDown;
        public Toggle toggle;
    }
    [SerializeField] PlayerSelect[] playerSelection;

    [SerializeField] GameObject warningBackground;
    [SerializeField] TMP_Text warningText;

    public void StartButtonEvent()
    {
        bool allPlayerAllowed = true;
        foreach (var player in playerSelection)
        {
            if (player.toggle.isOn && player.nameInput.text == "")
            {
                var placeholderText = player.nameInput.placeholder as TextMeshProUGUI;
                placeholderText.text = "<color=red><u>Oyuncu ismi girmeden oyun başlatılamaz!";
                allPlayerAllowed = false;
            }
        }

        if (!allPlayerAllowed)
            return;

        warningText.text = "";
        warningBackground.SetActive(false);
        foreach (var player in playerSelection)
        {
            if (player.toggle.isOn)
            {
                Setting newSet = new Setting(player.nameInput.text, player.typeDropDown.value, player.colorDropDown.value);

                if (!GameSettings.AddSetting(newSet, out string errorMsg))
                {
                    warningText.text = errorMsg;
                    warningBackground.SetActive(true);
                    return; // HATALI INPUT OLDUĞUNDA OYUNU BAŞLATMA
                }
            }
        }
        SceneManager.LoadScene("Game");
    }
}
