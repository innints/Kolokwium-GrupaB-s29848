namespace WebApplication1.Models.DTOs;

public class PolitykPartieGetDto
{
    public string Nazwa { get; set; }
    public string? Skrot { get; set; }
    public DateTime DataZalozenia { get; set; }
    public DateTime Od{ get; set; }
    public DateTime? Do{ get; set; }
}