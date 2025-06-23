using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public static class SeedData
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            if (!userManager.Users.Any(u => u.UserName == "Admin"))
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = "nhat2004hcm@gmail.com",
                    EmailConfirmed = true 
                };

                var result = await userManager.CreateAsync(adminUser, "123456");

                if (result.Succeeded)
                {
                    // Thêm vai trò Admin trong tương lai
                    // await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("Admin user created successfully.");
                }
                else
                {
                     Console.WriteLine("Failed to create admin user:");
                     foreach (var error in result.Errors)
                     {
                        Console.WriteLine($"- {error.Description}");
                     }
                }
            }
        }
    }
} 