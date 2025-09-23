namespace DexRobotPDA.DTOs;

public class AddDetect1Dto
{
    public string motor_id { get; set; }

    public DateTime combine_time { get; set; }
    
    public string? remarks { get; set; }

    public bool if_qualified { get; set; } = false;
}