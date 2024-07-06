using System;
using Assets.SimpleLocalization.Scripts;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

public class LanguageMgr : MMSingleton<LanguageMgr>
{
    // public Text FormattedText;

    /// <summary>
    /// Called on app start.
    /// </summary>
    public void Awake()
    {
        LocalizationManager.Read();
    }

    /// <summary>
    /// Change localization at runtime.
    /// <param name="localization">language name</param>>
    /// </summary>
    public void SetLocalization(string localization)
    {
        LocalizationManager.Language = localization;
    }

    /// <summary>
    /// Change localization at runtime between Chinese and English only.
    /// </summary>
    public void SetLocalization()
    {
        LocalizationManager.Language = (LocalizationManager.Language == "Chinese")
            ? "English"
            : "Chinese";
    }

    /// <summary>
    /// Play Dialog From .csv file
    /// </summary>
    public void PlayDialog(int index)
    {
        // FormattedText.text = LocalizationManager.Localize("Setting.DialogTest" + index);
    }
		
}