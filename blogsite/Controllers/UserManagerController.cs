using blogsite.Data;
using blogsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blogsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;
        private readonly ApplicationDbContext _context;  // DbContext'inizi ekliyoruz

        public UserManagementController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<UserManagementController> logger, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;  // DbContext'i constructor'a ekliyoruz
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userWithRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                
                // UserBlogCount verisini alıyoruz
                var userBlogCount = await _context.UserBlogCounts
                    .FirstOrDefaultAsync(ub => ub.email == user.Email);  // Kullanıcı ile ilişkilendiriyoruz

                userWithRoles.Add(new UserWithRolesViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList(),
                    BlogCount = userBlogCount?.blogcount ?? 0  // Blog sayısını ekliyoruz, null ise 0
                });
            }

            return View(userWithRoles);
        }

        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = new SelectList(_roleManager.Roles, "Name", "Name", roles);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, string newRole)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);

            return RedirectToAction(nameof(Index));
        }
    }
}
