public class PlayerDto
{
    public static GameDto gameDto { get; set; }
}
public class GameDto
{
    public string id { get; set; }
    public string username { get; set; }
    public long profileId { get; set; }

}