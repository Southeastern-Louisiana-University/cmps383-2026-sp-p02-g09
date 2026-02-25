using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using System.Security.Claims;

namespace Selu383.SP26.Api.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationsController(
    DataContext dataContext
    ) : ControllerBase
{
    [HttpGet]
    public IQueryable<LocationDto> GetAll()
    {
        return dataContext.Set<Location>()
            .Select(x => new LocationDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                TableCount = x.TableCount,
                ManagerId = x.ManagerId,
            });
    }

    [HttpGet("{id}")]
    public ActionResult<LocationDto> GetById(int id)
    {
        var result = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (result == null)
        {
            return NotFound();
        }

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
    public ActionResult<LocationDto> Create(LocationDto dto)
    {
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }

        var location = new Location
        {
            Name = dto.Name,
            Address = dto.Address,
            TableCount = dto.TableCount,
            ManagerId = dto.ManagerId,
        };

        dataContext.Set<Location>().Add(location);
        dataContext.SaveChanges();

        dto.Id = location.Id;

        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    [HttpPut("{id}")]
    [Authorize]
    public ActionResult<LocationDto> Update(int id, LocationDto dto)
    {
        if (dto.TableCount < 1)
        {
            return BadRequest();
        }

        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location == null)
        {
            return NotFound();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (location.ManagerId != userId && !User.IsInRole("Admin"))
            return Forbid();

        location.Name = dto.Name;
        location.Address = dto.Address;
        location.TableCount = dto.TableCount;
        location.ManagerId = dto.ManagerId;

        dataContext.SaveChanges();

        dto.Id = location.Id;

        return Ok(dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public ActionResult Delete(int id)
    {
        var location = dataContext.Set<Location>()
            .FirstOrDefault(x => x.Id == id);

        if (location == null)
        {
            return NotFound();
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (location.ManagerId != userId && !User.IsInRole("Admin"))
            return Forbid();

        dataContext.Set<Location>().Remove(location);
        dataContext.SaveChanges();

        return Ok();
    }
}
