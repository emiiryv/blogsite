namespace blogsite.Models
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int BlogCount { get; set; }
    }
}
