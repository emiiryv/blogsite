namespace blogsite.Models
{
public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<BlogCountByCategoryViewModel> BlogCounts { get; set; }
}

public class BlogCountByCategoryViewModel
{
    public string CategoryName { get; set; }
    public int BlogCount { get; set; }
    public int CommentCount { get; set; }
}
}