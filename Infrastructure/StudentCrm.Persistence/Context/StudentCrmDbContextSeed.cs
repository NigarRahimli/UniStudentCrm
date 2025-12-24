using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Student.Domain.Entities;

namespace StudentCrm.Persistence.Context
{
    public static class StudentCrmDbContextSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            const string adminRole = "Admin";

            // Role create
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await roleManager.CreateAsync(new AppRole { Name = adminRole });
                if (!roleResult.Succeeded)
                    throw new Exception(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }

            // Admin user
            var adminEmail = "admin@crm.local";
            var adminUserName = "admin";
            var adminPassword = "Admin123!";

            var adminUser =
                await userManager.FindByEmailAsync(adminEmail) ??
                await userManager.FindByNameAsync(adminUserName);

            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (!createResult.Succeeded)
                    throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));
            }

            // Assign role
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                var addRoleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                if (!addRoleResult.Succeeded)
                    throw new Exception(string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
