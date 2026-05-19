using AliMertRestoran.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication("AdminCookies")
    .AddCookie("AdminCookies", options =>
    {
        options.Cookie.Name = "AdminLoginCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(1);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Reset DB for demonstration to seed new rich data
    context.Database.EnsureDeleted();
    context.Database.Migrate();

    if (!context.Categories.Any())
    {
        var category1 = new Category { Name = "Geleneksel Kebaplar" };
        var category2 = new Category { Name = "Burger & Pizza" };
        var category3 = new Category { Name = "Tatlılar" };
        var category4 = new Category { Name = "Serinletici İçecekler" };

        context.Categories.AddRange(category1, category2, category3, category4);
        context.SaveChanges();

        context.Products.AddRange(
            new Product { Name = "Adana Kebap", Description = "Zırh kıymasından, acılı geleneksel lezzet. Közlenmiş biber ve domates ile.", Price = 350, ImageUrl = "https://images.unsplash.com/photo-1555939594-58d7cb561ad1?w=500&q=80", CategoryId = category1.Id },
            new Product { Name = "Urfa Kebap", Description = "Acısız, sade ve nefis Urfa kebabı. Sumaklı soğan eşliğinde.", Price = 340, ImageUrl = "https://images.unsplash.com/photo-1603360946369-dc9bb6258143?w=500&q=80", CategoryId = category1.Id },
            new Product { Name = "Beyti Sarma", Description = "Lavaş içinde özel sos ve yoğurtla harmanlanmış enfes beyti.", Price = 400, ImageUrl = "https://images.unsplash.com/photo-1628294895950-9805252327bc?w=500&q=80", CategoryId = category1.Id },
            
            new Product { Name = "Truffle Burger", Description = "180gr dana köfte, karamelize soğan, cheddar peyniri ve özel trüf mayonez.", Price = 280, ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=500&q=80", CategoryId = category2.Id },
            new Product { Name = "Margherita Pizza", Description = "İnce hamur, taze mozzarella, fesleğen ve özel pizza sosu.", Price = 250, ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=500&q=80", CategoryId = category2.Id },
            
            new Product { Name = "Fırın Sütlaç", Description = "Kavrulmuş fındık taneleriyle, tam kıvamında fırın sütlaç.", Price = 90, ImageUrl = "https://images.unsplash.com/photo-1624353365286-3f8d62daad51?w=500&q=80", CategoryId = category3.Id },
            new Product { Name = "San Sebastian", Description = "Akışkan iç dokusu ve yanık üstüyle meşhur cheesecake.", Price = 160, ImageUrl = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=500&q=80", CategoryId = category3.Id },
            
            new Product { Name = "Buzlu Nane Limonata", Description = "Taze nane yaprakları ve taze sıkılmış limon suyu.", Price = 65, ImageUrl = "https://images.unsplash.com/photo-1513558161293-cdaf765ed2fd?w=500&q=80", CategoryId = category4.Id },
            new Product { Name = "Soğuk Kahve (Iced Latte)", Description = "Buz gibi süt ve espresso.", Price = 85, ImageUrl = "https://images.unsplash.com/photo-1461023058943-07cb1ce8e7dd?w=500&q=80", CategoryId = category4.Id }
        );
        context.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
