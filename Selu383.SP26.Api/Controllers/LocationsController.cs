using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using System.Security.Claims;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(DataContext dataContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll()
    {
        var locations = await dataContext.Set<Location>()
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                TableCount = x.TableCount,
                ManagerId = x.ManagerId,
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetById(int id)
    {
        var result = await dataContext.Set<Location>().FirstOrDefaultAsync(x => x.Id == id);

        if (result == null) return NotFound();

        return Ok(new LocationDto
        {
            Id = result.Id,
            Name = result.Name,
            Address = result.Address,
            TableCount = result.TableCount,
            ManagerId = result.ManagerId,
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LocationDto>> Create(LocationDto dto)
    {
        if (dto.TableCount < 1) return BadRequest("Table count must be at least 1.");

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId,
        };

        dataContext.Set<Location>().Add(location);
        await dataContext.SaveChangesAsync();

        dto.Id = location.Id;

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<LocationDto>> Update(int id, LocationDto dto)
    {
        if (dto.TableCount < 1) return BadRequest("Table count must be at least 1.");

        var location = await dataContext.Set<Location>().FirstOrDefaultAsync(x => x.Id == id);

        if (location == null) return NotFound();
        if (!IsAuthorizedToModify(location)) return Forbid();

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.TableCount = dto.TableCount;
        location.ManagerId = dto.ManagerId;

        await dataContext.SaveChangesAsync();
        dto.Id = location.Id;

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(int id)
    {
        var location = await dataContext.Set<Location>().FirstOrDefaultAsync(x => x.Id == id);

        if (location == null) return NotFound();
        if (!IsAuthorizedToModify(location)) return Forbid();

        dataContext.Set<Location>().Remove(location);
        await dataContext.SaveChangesAsync();

        return Ok();
    }

    // Helper method to keep authorization logic in one place
    private bool IsAuthorizedToModify(Location location)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdString, out int userId))
        {
            return location.ManagerId == userId || User.IsInRole("Admin");
        }
        return false;
    }
}
