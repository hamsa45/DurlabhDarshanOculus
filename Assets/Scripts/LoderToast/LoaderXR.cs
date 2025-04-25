using UnityEngine;
public class LoaderXR
{
    public static GameObject loader;
    public void CreateLoader()
    {
        loader = GameObject.Instantiate(Resources.Load("LoaderCanvas") as GameObject);
    }
    public void StartLoader()
    {
        loader.transform.GetChild(0).gameObject.SetActive(true);
    }
    public void StopLoader()
    {
        loader.transform.GetChild(0).gameObject.SetActive(false);
    }


}
