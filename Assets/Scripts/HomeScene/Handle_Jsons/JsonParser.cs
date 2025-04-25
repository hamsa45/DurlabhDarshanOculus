using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class JsonParser<T> : IJsonParser<T>
{
    public T ParseSingle(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse single JSON object: {ex.Message}");
            return default;
        }
    }

    public List<T> ParseList(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to parse JSON list: {ex.Message}");
            return new List<T>();
        }
    }
}
