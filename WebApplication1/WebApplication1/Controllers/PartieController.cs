using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers;


[ApiController]
[Route("[controller]")]
public class PartieController(IDbService dbService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePartiaWithPolitycy(
        [FromBody] PartiaWithPolitycyCreateDto body
    )
    {
        
        var client = await dbService.CreatePartiaWithPolitycyAsync(body);
        return Created($"partie/{client.ID}", client);
        
      
    }
}