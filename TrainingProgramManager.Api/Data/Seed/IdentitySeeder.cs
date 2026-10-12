using Microsoft.AspNetCore.Identity;
using TrainingProgramManager.Api.Security;

namespace TrainingProgramManager.Api.Data.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in ApplicationRoles.All)
            {
                // EN: The existence check keeps the seeder idempotent: repeated startups never recreate roles.
                // HU: A létezés-ellenőrzés miatt a seeder idempotens: az ismételt indítások nem hozzák létre újra a szerepköröket.
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(new IdentityRole(roleName));

                if (!result.Succeeded)
                {
                    var errors = string.Join(" ", result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }
    }
}
