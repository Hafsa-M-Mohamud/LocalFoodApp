using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Assignment3BAD.Models;
using Assignment3BAD.Database;
using Assignment3BAD.Seeding;
using Microsoft.AspNetCore.Authorization;


namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController : ControllerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public SeedController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedDatabase()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDBContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                await db.Database.MigrateAsync();
                await SeedDummyData.seedDb(db);
                await SeedRolesAndUser.SeedRolesAndUsers(userManager, roleManager);
                await SeedCooks.SeedCooksAndUsers(db, userManager);
                await SeedCyclist.SeedCyclistsAndUsers(db, userManager); // Tilføjet denne linje
            }

            return Ok("Database seeded successfully.");
        }
    }
}