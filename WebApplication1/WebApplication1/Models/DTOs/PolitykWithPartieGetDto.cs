namespace WebApplication1.Models.DTOs;

public class PolitykWithPartieGetDto
{
    public int ID { get; set; }
    public string Imie { get; set; }
    public string Nazwisko { get; set; }
    public string Powiedzenie { get; set; }
    
    public List<PolitykPartieGetDto>? Przynaleznosc { get; set; }

}