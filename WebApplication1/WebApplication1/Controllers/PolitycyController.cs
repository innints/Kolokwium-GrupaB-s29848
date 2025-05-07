namespace WebApplication1.Controllers;

using System.Threading.Tasks;
using WebApplication1.Services;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("[controller]")]
public class PolitycyController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPolitycyWithPartie(
    )
    {
        
        return Ok(await dbService.GetPolitycyWithPartieAsync());
        
      
    }
}