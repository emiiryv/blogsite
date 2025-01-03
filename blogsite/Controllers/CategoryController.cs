using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using blogsite.Data;
using blogsite.Models;
using Microsoft.AspNetCore.Authorization;

namespace blogsite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Category
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    BlogCounts = _context.BlogCountByCategory
                        .Where(b => b.categoryid == c.Id)
                        .Select(b => new BlogCountByCategoryViewModel
                        {
                            CategoryName = b.categoryname,
                            BlogCount = b.blogcount,
                            // Burada CommentCount sıfır olarak başlatılacak, sonra güncellenecek
                            CommentCount = 0
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound();
            }

            // BlogCounts listesindeki her öğe için yorum sayısını ekleyelim
            foreach (var item in category.BlogCounts)
            {
                // Blog ile ilişkili yorum sayısını hesapla
                var commentCount = await _context.Comments
                    .Where(com => com.Blog.CategoryId == id) // Burada CategoryId'yi kullanarak ilişkilendirme yapıyoruz
                    .CountAsync();

                // Yorum sayısını ekleyelim
                item.CommentCount = commentCount;
            }

            return View(category);
        }









        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Blogs) // Kategoriye ait blogları dahil et
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Blogs.Any())
            {
                TempData["ErrorMessage"] = "This category cannot be deleted because it has associated blogs.";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
