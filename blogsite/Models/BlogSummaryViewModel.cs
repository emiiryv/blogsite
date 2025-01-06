namespace blogsite.Models
{
    // En Çok Yorum Alan Bloglar için ViewModel
    public class MostCommentedBlogViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public int CommentCount { get; set; } // Yorum sayısı
        public string ImageUrl { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; }
    }

    // En Uzun Bloglar için ViewModel
    public class LongestBlogViewModel
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public int ContentLength { get; set; } // İçerik uzunluğu
        public string ImageUrl { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; }
    }
}
