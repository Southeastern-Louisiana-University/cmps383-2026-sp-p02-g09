using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Users;

public class UserRole : IdentityUserRole<int>
{
    public Role Role { get; set; } = null!;
    public User User { get; set; } = null!;
}
