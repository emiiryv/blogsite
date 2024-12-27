using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogsite.Models;
using blogsite.Data;

namespace blogsite.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Blogları kategori bilgileriyle birlikte getir
        var blogs = await _context.Blogs
            .Include(b => b.Category) // Kategori bilgilerini dahil et
            .OrderByDescending(b => b.CreatedAt) // En son eklenen bloglar en başta
            .ToListAsync();

        return View(blogs);
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
