namespace blogsite.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int BlogId { get; set; } // Yorumun ait olduğu blog

        public string Content { get; set; } = string.Empty; // Yorum içeriği
        public string? Author { get; set; } // Yorumu yapan kişi
        public DateTime CreatedAt { get; set; }

        // Blog ile ilişki
        public Blog Blog { get; set; }
    }
}
