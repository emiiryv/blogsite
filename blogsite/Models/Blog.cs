using System;
using System.ComponentModel.DataAnnotations;

namespace blogsite.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        [Required]
        public int CategoryId { get; set; } // Foreign key

        public Category? Category { get; set; } // Navigation property

        // Yeni eklenen alan
        public string? ImageUrl { get; set; }

        public ICollection<Comment>? Comments { get; set; }
        

    }
}
