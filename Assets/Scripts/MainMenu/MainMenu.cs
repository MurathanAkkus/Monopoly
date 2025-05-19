
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

    public void StartButtonEvent()
    {
        foreach (var player in playerSelection)
        {
            if (player.toggle.isOn)
            {
                Setting newSet = new Setting(player.nameInput.text, player.typeDropDown.value, player.colorDropDown.value);
                GameSettings.AddSetting(newSet);
            }
        }
        SceneManager.LoadScene("Game");
    }
}
