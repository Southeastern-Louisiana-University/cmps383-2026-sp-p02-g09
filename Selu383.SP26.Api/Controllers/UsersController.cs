using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager
) : ControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName)) return BadRequest("Username is required.");
        if (string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Password is required.");
        if (dto.Roles == null || dto.Roles.Length == 0) return BadRequest("At least one role is required.");

        foreach (var role in dto.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                return BadRequest($"Role '{role}' does not exist.");
        }

        var user = new User { UserName = dto.UserName };
        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await userManager.AddToRolesAsync(user, dto.Roles);

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Roles = dto.Roles
        };

        // Standard REST practice: Return HTTP 201 Created for POST requests
        return StatusCode(201, userDto); 
    }
}
