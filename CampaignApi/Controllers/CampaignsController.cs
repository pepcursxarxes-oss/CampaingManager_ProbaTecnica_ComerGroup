using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;

namespace CampaignApi.Controllers;

[ApiController]
[Route("campaigns")]
public sealed class CampaignsController : ControllerBase
{
    private readonly ICampanyaService _service;
    public CampaignsController(ICampanyaService service )
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Crear([FromBody]CrearCampanyaDTO dto, CancellationToken cancellationToken) 
    {
        try 
        {
            var id = _service.CrearAsync(dto, cancellationToken);

            return CreatedAtAction(nameof(ObtenirPerId), new { id }, new { id });
        }
        catch (Exception ex) 
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CampanyaDTO>>> Llistar([FromQuery] EstatCampanya? estat, CancellationToken cancellationToken)
    {
        var campanyes = await _service.LlistarAsync(estat, cancellationToken);

        return Ok(campanyes);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CampanyaDTO>> ObtenirPerId(Guid id, CancellationToken cancellationToken)
    { 
        var campanya = await _service.ObtenirPerIdAsync(id, cancellationToken);

        if (campanya == null) 
        {
            return NotFound(new { error = $"No s'ha trobat la campanya {id}"});
        }
        return Ok(campanya);
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CanviarEstat(
        Guid id,
        [FromBody] CanviarEstatCampanyaDTO dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.CanviarEstatAsync(
                id, dto.Estat, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/consume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Consumir(
        Guid id,
        [FromBody] ConsumirPressupostDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.ConsumirAsync(
                id, dto.Import, cancellationToken);

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }





    }
