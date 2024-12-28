using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using blogsite.Data;
using System.Threading.Tasks;

namespace blogsite.ViewComponents
{
    public class RelatedBlogsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public RelatedBlogsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int categoryId, int currentBlogId)
        {
            // Aynı kategoriye ait diğer blogları getir
            var relatedBlogs = await _context.Blogs
                .Where(b => b.CategoryId == categoryId && b.Id != currentBlogId)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5) // Yalnızca son 5 blog
                .ToListAsync();

            return View(relatedBlogs);
        }
    }
}
