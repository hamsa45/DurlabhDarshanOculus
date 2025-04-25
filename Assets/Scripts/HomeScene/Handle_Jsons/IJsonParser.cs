using System.Collections.Generic;

public interface IJsonParser<T>
{
    T ParseSingle(string json);
    List<T> ParseList(string json);
}
