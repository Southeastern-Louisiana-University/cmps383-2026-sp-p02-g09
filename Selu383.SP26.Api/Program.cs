using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP26.Api.Data;
using Selu383.SP26.Api.Features.Locations;
using Selu383.SP26.Api.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DataContext")));

builder.Services.AddIdentity<User, Role>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    await db.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    // Seed roles
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new Role { Name = "Admin" });
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new Role { Name = "User" });
    }

    // Seed users
    if (await userManager.FindByNameAsync("galkadi") == null)
    {
        var admin = new User { UserName = "galkadi" };
        await userManager.CreateAsync(admin, "Password123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
    if (await userManager.FindByNameAsync("bob") == null)
    {
        var bob = new User { UserName = "bob" };
        await userManager.CreateAsync(bob, "Password123!");
        await userManager.AddToRoleAsync(bob, "User");
    }
    if (await userManager.FindByNameAsync("sue") == null)
    {
        var sue = new User { UserName = "sue" };
        await userManager.CreateAsync(sue, "Password123!");
        await userManager.AddToRoleAsync(sue, "User");
    }

    // Seed locations
    if (!db.Locations.Any())
    {
        db.Locations.AddRange(
            new Location { Name = "Location 1", Address = "123 Main St", TableCount = 10 },
            new Location { Name = "Location 2", Address = "456 Oak Ave", TableCount = 20 },
            new Location { Name = "Location 3", Address = "789 Pine Ln", TableCount = 15 }
        );
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseEndpoints(x =>
    {
        x.MapControllers();
    });

app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSpa(x =>
    {
        x.UseProxyToSpaDevelopmentServer("http://localhost:5173");
    });
}
else
{
    app.MapFallbackToFile("/index.html");
}

app.Run();

//see: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0
// Hi 383 - this is added so we can test our web project automatically
public partial class Program { }
