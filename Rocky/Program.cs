using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Utility;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorPages(
//options =>
//{
//    options.Conventions.AddPageRoute("/Archive/Post", "Post/{year}/{month}/{day}/{title}");
//}
    //).AddRazorPagesOptions(options =>
    //{
    //    options.Conventions.AddPageRoute("/index", "{*url}");
    //}
    );

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDefaultIdentity<IdentityUser>()
    // Attention! The sequence of addition is important!I
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders().AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    ;
builder.Services.AddTransient<IEmailSender, EmailSender>(); 
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(Options =>
{ 
    Options.IdleTimeout = TimeSpan.FromMinutes(10);
    Options.Cookie.HttpOnly = true; 
    Options.Cookie.IsEssential = true;  
});


var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.MapRazorPages();

app.MapControllerRoute(
    //name: "default",
    //pattern: "{controller=Home}/{action=Index}/{id?}"
    name: "area",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    );

app.MapAreaControllerRoute(
               name: "Main",
               areaName: "Main",
               pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
