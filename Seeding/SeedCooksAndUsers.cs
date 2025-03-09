using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Assignment3BAD.Database;
using Assignment3BAD.Models;

namespace Assignment3BAD.Seeding{
public static class SeedCooks
{
    public static async Task SeedCooksAndUsers(MyDBContext db, UserManager<ApplicationUser> userManager)
    {
        Console.WriteLine("Starter seeding af kokke og brugere");
        
        var cooks = await db.Cooks.ToListAsync();
        Console.WriteLine($"Fandt {cooks.Count} kokke i databasen");

        foreach (var cook in cooks)
        {
            var email = $"cook{cook.CookID}@example.com";
            Console.WriteLine($"Behandler kok {cook.CookID} med email {email}");

            var existingUser = await userManager.FindByEmailAsync(email);
            
            if (existingUser == null)
            {
                Console.WriteLine($"Opretter ny bruger for kok {cook.CookID}");
                var newUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Role = "Cook"
                };

                var result = await userManager.CreateAsync(newUser, "Cook123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, "Cook");
                    cook.UserId = newUser.Id;
                    db.Cooks.Update(cook);
                    Console.WriteLine($"Oprettet bruger med ID {newUser.Id} for kok {cook.CookID}");
                }
                else
                {
                    Console.WriteLine($"Fejl ved oprettelse af bruger for kok {cook.CookID}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Fandt eksisterende bruger for kok {cook.CookID}");
                cook.UserId = existingUser.Id;
                db.Cooks.Update(cook);
            }
        }

        await db.SaveChangesAsync();
        Console.WriteLine("Afsluttet seeding af kokke og brugere");
    }
}
}
