using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Student.Domain.Entities;


namespace StudentCrm.Persistence.Context
{
    public static class StudentCrmDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<Admin>>();

            const string adminRole = "Admin";
            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole(adminRole));

            var adminEmail = "admin@crm.local";
            var adminUserName = "admin";
            var adminPassword = "Admin123!";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail) ??
                await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new Admin
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Name = "System",
                    Surname = "Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (!result.Succeeded)
                    throw new Exception(string.Join("; ", result.Errors.Select(e => e.Description)));
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
    }
