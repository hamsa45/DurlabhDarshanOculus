using UnityEditor;
using System.Collections;

[System.Serializable]
public class NewCountryCode
{
    public int id { get; set; }
    public string name { get; set; }
    public string isoCode { get; set; }
    public int dialingCode { get; set; }
}


[System.Serializable]
public class AssetBundleLink
{
    public string ExperienceName { get; set; }
    public string Link { get; set; }
}