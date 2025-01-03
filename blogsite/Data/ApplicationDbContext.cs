using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using blogsite.Models;

namespace blogsite.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanı tabloları için DbSet tanımları
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Blog> ActiveBlogs { get; set; }
        public DbSet<BlogCountByCategory> BlogCountByCategory { get; set; }
        public DbSet<UserBlogCount> UserBlogCounts { get; set; }
        public DbSet<BlogWithCommentCount> BlogWithCommentCounts { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BlogWithCommentCount>().ToView("BlogWithCommentCount");


            modelBuilder.Entity<UserBlogCount>(entity =>
        {
            entity.HasKey(u => u.row_id);  // RowId'yi anahtar olarak belirliyoruz
            entity.ToView("userblogcount");  // View adı küçük harflerle belirtilmeli
        });

            modelBuilder.Entity<BlogCountByCategory>()
            .HasKey(b => b.id);  // Yeni ID'yi anahtar olarak belirliyoruz



            modelBuilder.Entity<Blog>().ToView("ActiveBlogs").HasKey(b => b.Id);

            // Identity tablolarını özelleştirme
            modelBuilder.Entity<IdentityUser>(b =>
            {
                b.ToTable("Users"); // AspNetUsers yerine Users tablosunu kullan
            });

            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("Roles"); // AspNetRoles yerine Roles tablosunu kullan
            });

            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("UserRoles"); // AspNetUserRoles yerine UserRoles
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("UserClaims"); // AspNetUserClaims yerine UserClaims
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("UserLogins"); // AspNetUserLogins yerine UserLogins
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("RoleClaims"); // AspNetRoleClaims yerine RoleClaims
            });

            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("UserTokens"); // AspNetUserTokens yerine UserTokens
            });

            // Blog tablosu için CreatedAt sütununun tipini ve varsayılan değerini ayarla
            modelBuilder.Entity<Blog>()
                .Property(b => b.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");

            // Comment tablosu için CreatedAt sütununun tipini ve varsayılan değerini ayarla
            modelBuilder.Entity<Comment>()
                .Property(c => c.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");

            // Blog-Comment ilişkisi için Cascade Delete kuralı
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
