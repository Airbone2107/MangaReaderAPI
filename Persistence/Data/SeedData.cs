using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Persistence.Data
{
    public static class SeedData
    {
        public static async Task SeedEssentialsAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed SuperAdmin User
            await SeedSuperAdminAsync(userManager, roleManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            // Tạo các vai trò từ AppRoles constants
            await CreateRoleIfNotExists(roleManager, AppRoles.SuperAdmin);
            await CreateRoleIfNotExists(roleManager, AppRoles.Admin);
            await CreateRoleIfNotExists(roleManager, AppRoles.Moderator);
            await CreateRoleIfNotExists(roleManager, AppRoles.User);
        }

        private static async Task CreateRoleIfNotExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Tạo user SuperAdmin mặc định
            var defaultUser = new ApplicationUser 
            { 
                UserName = "superadmin", 
                Email = "superadmin@mangareader.com", 
                EmailConfirmed = true 
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "123456");
                    await userManager.AddToRoleAsync(defaultUser, AppRoles.SuperAdmin);
                    await userManager.AddToRoleAsync(defaultUser, AppRoles.Admin);
                    await userManager.AddToRoleAsync(defaultUser, AppRoles.Moderator);
                    await userManager.AddToRoleAsync(defaultUser, AppRoles.User);
                }
            }
            
            // Gán tất cả quyền cho vai trò SuperAdmin
            await AddAllPermissionsToRole(roleManager, AppRoles.SuperAdmin);
        }
        
        public static async Task AddAllPermissionsToRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null) return;
            
            var allClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = GetAllPermissions();
            
            foreach (var permission in allPermissions)
            {
                if (!allClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim("permission", permission));
                }
            }
        }

        private static List<string> GetAllPermissions()
        {
            var permissions = new List<string>();
            var nestedTypes = typeof(Permissions).GetNestedTypes();
            foreach (var type in nestedTypes)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                permissions.AddRange(fields.Select(fi => (string)fi.GetValue(null)!));
            }
            return permissions;
        }
    }
} 