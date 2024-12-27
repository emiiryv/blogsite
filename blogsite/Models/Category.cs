using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace blogsite.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        public string Name { get; set; }

        // Navigation property - nullable olarak ayarlandı
        public ICollection<Blog>? Blogs { get; set; }
    }
}
