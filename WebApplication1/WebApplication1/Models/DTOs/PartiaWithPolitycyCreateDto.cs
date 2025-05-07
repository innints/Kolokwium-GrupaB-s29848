namespace WebApplication1.Models.DTOs;

public class PartiaWithPolitycyCreateDto
{
    public string Nazwa { get; set; }
    public string? Skrot { get; set; }
    public DateTime DataZalozenia { get; set; }
    public List<int>? Czlonkowie { get; set; }
}