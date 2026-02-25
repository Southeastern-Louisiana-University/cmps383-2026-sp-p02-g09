using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Users;

public class Role : IdentityRole<int>
{
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}
