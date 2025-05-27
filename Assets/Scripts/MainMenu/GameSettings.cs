using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSettings
{
    public static List<Setting> settingsList = new List<Setting>();

    public static bool AddSetting(Setting setting, out string errorMsg)
    {
        if (settingsList.Exists(s => s.playerName == setting.playerName))
        {
            errorMsg = $"Aynı isme sahip oyuncu zaten var: <u>{setting.playerName}</u>";
            return false;
        }

        if (settingsList.Exists(s => s.selectedColor == setting.selectedColor))
        {
            errorMsg = $"Bu renk zaten seçildi: <u>{setting.selectedColor}</u>";
            return false;
        }

        settingsList.Add(setting);
        errorMsg = null;
        return true;
    }
}

public class Setting
{
    public string playerName;
    public int selectedType;
    public int selectedColor;

    public Setting(string _name, int _type, int _color)
    {
        playerName = _name;
        selectedType = _type;
        selectedColor = _color;
    }
}