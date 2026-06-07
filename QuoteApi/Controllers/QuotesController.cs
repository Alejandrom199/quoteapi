using Microsoft.AspNetCore.Mvc;

namespace QuoteApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuotesController : ControllerBase
{
    private static readonly string[] Quotes =
    {
        "La mejor forma de predecir el futuro es construirlo.",
        "El código limpio siempre parece que fue escrito por alguien que se preocupa.",
        "Automatiza todo lo que hagas más de dos veces.",
        "Un sistema complejo que funciona evolucionó de uno simple que funcionaba.",
        "El mejor error es el que nunca llega a producción."
    };

    [HttpGet("random")]
    public IActionResult GetRandom()
    {
        var quote = Quotes[Random.Shared.Next(Quotes.Length)];
        return Ok(new
        {
            quote,
            timestamp = DateTime.UtcNow,
            host = Environment.MachineName
        });
    }

    [HttpGet]
    public IActionResult GetAll() => Ok(new { total = Quotes.Length, quotes = Quotes });

}