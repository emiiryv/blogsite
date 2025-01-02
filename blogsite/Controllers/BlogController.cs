using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogsite.Data;
using blogsite.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;

namespace blogsite.Controllers
{
    [Authorize] // Genel olarak giriş yapmış tüm kullanıcılar için erişilebilir
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Blog
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        public async Task<IActionResult> Index()
        {
            var blogs = User.IsInRole("Admin")
                ? _context.Blogs.Include(b => b.Category)
                : _context.Blogs.Include(b => b.Category).Where(b => b.CreatedBy == User.Identity.Name);

            return View(await blogs.ToListAsync());
        }

        // GET: Blog/Create
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Blog/Create
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,CategoryId")] Blog blog, IFormFile? image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Görsel Yükleme İşlemi
                    string? imageUrl = null;
                    if (image != null)
                    {
                        var uploadsFolder = Path.Combine("wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder); // Eğer klasör yoksa oluştur
                        }

                        var filePath = Path.Combine(uploadsFolder, image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        imageUrl = $"/images/{image.FileName}";
                    }

                    // Stored Procedure için Parametreler
                    var parameters = new[]
                    {
                new NpgsqlParameter("p_Title", blog.Title),
                new NpgsqlParameter("p_Content", blog.Content),
                new NpgsqlParameter("p_CategoryId", blog.CategoryId),
                new NpgsqlParameter("p_ImageUrl", (object?)imageUrl ?? DBNull.Value)
            };

                    // Stored Procedure'ü Çağırma
                    await _context.Database.ExecuteSqlRawAsync("CALL addblog(@p_Title, @p_Content, @p_CategoryId, @p_ImageUrl);", parameters);

                    TempData["SuccessMessage"] = "Blog başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                    ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            // Kategori seçimi için tekrar verileri gönder
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blog.CategoryId);
            return View(blog);
        }




        // GET: Blog/Edit/5
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null || (!User.IsInRole("Admin") && blog.CreatedBy != User.Identity.Name))
            {
                return Forbid();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blog.CategoryId);
            return View(blog);
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CategoryId")] Blog blog, IFormFile? image)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        var filePath = Path.Combine("wwwroot/images", image.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }
                        blog.ImageUrl = $"/images/{image.FileName}";
                    }

                    var existingBlog = await _context.Blogs.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    if (existingBlog != null && !User.IsInRole("Admin") && existingBlog.CreatedBy != User.Identity.Name)
                    {
                        return Forbid();
                    }

                    blog.CreatedAt = existingBlog.CreatedAt;
                    blog.CreatedBy = existingBlog.CreatedBy;

                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blog.CategoryId);
            return View(blog);
        }

        // GET: Blog/Delete/5
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Editor")] // Sadece Admin ve Editor için
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Stored procedure çağrısı
                var sql = "CALL DeleteBlog({0})";
                await _context.Database.ExecuteSqlRawAsync(sql, id);

                TempData["SuccessMessage"] = "Blog başarıyla silindi.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                TempData["ErrorMessage"] = "Blog silinirken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: Blog/Details/5
        [Authorize] // Giriş yapan herhangi bir kullanıcı erişebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Category)
                .Include(b => b.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blog/AddComment
        [Authorize] // Giriş yapan herhangi bir kullanıcı yorum yapabilir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int BlogId, string Content)
        {
            if (string.IsNullOrEmpty(Content))
            {
                ModelState.AddModelError("Content", "Comment cannot be empty.");
                return RedirectToAction("Details", new { id = BlogId });
            }

            var comment = new Comment
            {
                BlogId = BlogId,
                Content = Content,
                Author = User.Identity?.Name ?? "Anonymous",
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = BlogId });
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
