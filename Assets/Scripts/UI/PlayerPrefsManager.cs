using UnityEngine;

public static class PlayerPrefsManager
{
    // Category related methods
    public static void SaveSelectedCategory(string category)
    {
        PlayerPrefs.SetString(PlayerPrefsConst.SELECTED_CATEGORY, category);
        PlayerPrefs.Save();
    }

    public static string LoadSelectedCategory()
    {
        return PlayerPrefs.GetString(PlayerPrefsConst.SELECTED_CATEGORY, "All");
    }

    // Navigation panel related methods
    public static void SaveSelectedNavPanel(int index)
    {
        PlayerPrefs.SetInt(PlayerPrefsConst.LAST_SELECTED_NAV_PANEL, index);
        PlayerPrefs.Save();
    }

    public static int LoadSelectedNavPanel()
    {
        return PlayerPrefs.GetInt(PlayerPrefsConst.LAST_SELECTED_NAV_PANEL, 0);
    }

    // Audio related methods
    public static void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat(PlayerPrefsConst.VOLUME, volume);
        PlayerPrefs.Save();
    }

    public static float LoadVolume()
    {
        return PlayerPrefs.GetFloat(PlayerPrefsConst.VOLUME, 1f);
    }

    // Graphics related methods
    public static void SaveQualityLevel(int level)
    {
        PlayerPrefs.SetInt(PlayerPrefsConst.QUALITY, level);
        PlayerPrefs.Save();
    }

    public static int LoadQualityLevel()
    {
        return PlayerPrefs.GetInt(PlayerPrefsConst.QUALITY, QualitySettings.GetQualityLevel());
    }

    // Clear all preferences
    public static void ClearAllPreferences()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
} 