using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/authentication")]
[ApiController]
public class AuthenticationController(
    UserManager<User> userManager,
    SignInManager<User> signInManager
) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto dto)
    {
        var user = await userManager.FindByNameAsync(dto.UserName);
        if (user == null)
        {
            return BadRequest("Invalid username or password.");
        }

        var result = await signInManager.PasswordSignInAsync(user, dto.Password, isPersistent: true, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return BadRequest("Invalid username or password.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Roles = roles.ToArray()
        });
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        if (!User.Identity!.IsAuthenticated)
        {
            return Unauthorized();
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Roles = roles.ToArray()
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok();
    }
}
