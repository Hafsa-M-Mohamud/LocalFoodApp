using Assignment3BAD.Database;
using Assignment3BAD.Models;
using Assignment3BAD.Controllers;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment3BAD.Seeding
{
    public static class SeedCyclist
    {
        public static async Task SeedCyclistsAndUsers(MyDBContext db, UserManager<ApplicationUser> userManager)
        {
            var cyclists = await db.Cyclists.ToListAsync();

            foreach (var cyclist in cyclists)
            {
                // Check if this Cyclist already has a User associated
                if (string.IsNullOrEmpty(cyclist.UserId))
                {
                    // Generate a unique email based on CyclistID
                    var email = $"cyclist{cyclist.CyclistID}@example.com";

                    // Check if the user already exists
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser == null)
                    {
                        // Create a new user
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            Role = "Cyclist"
                        };

                        var result = await userManager.CreateAsync(newUser, "Cyclist123!"); // Default password
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newUser, "Cyclist");

                            // Associate UserId with Cyclist
                            cyclist.UserId = newUser.Id;
                            db.Cyclists.Update(cyclist);
                        }
                        else
                        {
                            Console.WriteLine($"Failed to create user for CyclistID {cyclist.CyclistID}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                    else
                    {
                        // If the user exists, associate UserId
                        cyclist.UserId = existingUser.Id;
                        db.Cyclists.Update(cyclist);
                    }
                }
            }

            await db.SaveChangesAsync();
            Console.WriteLine("All Cyclists now have associated users.");
        }
    }
}
