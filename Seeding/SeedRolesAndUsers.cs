using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Identity;
using Assignment3BAD.Models;

namespace Assignment3BAD.Seeding
{
    public static class SeedRolesAndUser
    {
        public static async Task SeedRolesAndUsers(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Define roles
            string[] roles = { "Admin", "Manager", "Cook", "Cyclist" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed Admin user
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    Role = "Admin"
                };
                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed Manager user
            if (await userManager.FindByEmailAsync("manager@example.com") == null)
            {
                var managerUser = new ApplicationUser
                {
                    UserName = "manager@example.com",
                    Email = "manager@example.com",
                    Role = "Manager"
                };
                await userManager.CreateAsync(managerUser, "Manager123!");
                await userManager.AddToRoleAsync(managerUser, "Manager");
            }

            // Add more users as needed
        }
    }
}