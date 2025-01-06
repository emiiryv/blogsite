using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogsite.Models;
using blogsite.Data;

namespace blogsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Anasayfa: Blogları default olarak getiriyoruz (son eklenenler)
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Category)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(blogs);
        }

        public async Task<IActionResult> MostCommentedBlogs(int limit = 5)
        {
            var mostCommentedBlogs = await _context.Set<MostCommentedBlogViewModel>()
                .FromSqlInterpolated($"SELECT * FROM getMostCommentedBlogs({limit})")
                .ToListAsync();

            return View("FilteredBlogs", mostCommentedBlogs);
        }

        public async Task<IActionResult> LongestBlogs(int limit = 5)
        {
            var longestBlogs = await _context.Set<LongestBlogViewModel>()
                .FromSqlInterpolated($"SELECT * FROM getLongestBlogs({limit})")
                .ToListAsync();

            return View("FilteredBlogs", longestBlogs);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
