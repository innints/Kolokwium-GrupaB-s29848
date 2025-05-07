namespace WebApplication1.Models.DTOs;

public class PartiaDetailsGetDto
{
    public int ID { get; set; }
    public string Nazwa { get; set; }
    public string? Skrot { get; set; }
    public DateTime DataZalozenia { get; set; }
    public List<PolitykGetDto>? Politycy { get; set; }
}