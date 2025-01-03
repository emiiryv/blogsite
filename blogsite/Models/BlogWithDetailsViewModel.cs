namespace blogsite.Models
{
    public class BlogWithDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; }
        public int CommentCount { get; set; }
    }
}
