using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogsite.Data;
using blogsite.Models;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace blogsite.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Blog
        public async Task<IActionResult> Index()
        {
            var blogs = User.IsInRole("Admin")
                ? _context.Blogs.Include(b => b.Category)
                : _context.Blogs.Include(b => b.Category).Where(b => b.CreatedBy == User.Identity.Name);

            return View(await blogs.ToListAsync());
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CategoryId")] Blog blog, IFormFile? image)
        {
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

                    blog.CreatedBy = User.Identity.Name ?? "UnknownUser";
                    blog.CreatedAt = DateTime.UtcNow;

                    _context.Add(blog);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Hata: " + ex.Message);
                    ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen tekrar deneyin.");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blog.CategoryId);
            return View(blog);
        }

        // GET: Blog/Edit/5
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Category) // Kategori bilgisi dahil
                .Include(b => b.Comments) // Yorum bilgileri (varsa)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
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
