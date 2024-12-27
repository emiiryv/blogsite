using Microsoft.EntityFrameworkCore;
using blogsite.Data; // ApplicationDbContext'in bulunduğu namespace
using blogsite.Models; // ApplicationUser'in bulunduğu namespace
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Şifre gereksinimlerini özelleştir
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;

    // E-posta doğrulama gereksinimini kaldır
    options.SignIn.RequireConfirmedEmail = false;
});

// Authentication ve Cookie ayarları
builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

var app = builder.Build();

// Middleware yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Statik dosyalar için middleware

app.UseRouting();

app.UseAuthentication(); // Kimlik doğrulama
app.UseAuthorization();  // Yetkilendirme

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Gerekli rolleri ve admin kullanıcısını oluşturma
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Admin rolünü oluştur
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Admin kullanıcısını oluştur
    var adminUser = await userManager.FindByEmailAsync("admin@blogsite.com");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = "admin@blogsite.com",
            Email = "admin@blogsite.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, "Admin123!");
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
    // Editor rolünü oluştur
    if (!await roleManager.RoleExistsAsync("Editor"))
    {
        await roleManager.CreateAsync(new IdentityRole("Editor"));
    }

    // Editor kullanıcısını oluştur
    var editorUser = await userManager.FindByEmailAsync("editor@blogsite.com");
    if (editorUser == null)
    {
        editorUser = new ApplicationUser
        {
            UserName = "editor@blogsite.com",
            Email = "editor@blogsite.com",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(editorUser, "Editor123!");
        await userManager.AddToRoleAsync(editorUser, "Editor");
    }

}

app.Run();
