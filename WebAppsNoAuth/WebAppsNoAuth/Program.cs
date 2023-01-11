using System.Data.SQLite;
using Microsoft.EntityFrameworkCore;
using WebAppsNoAuth.Data;
using WebAppsNoAuth.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using System.Diagnostics;
using WebAppsNoAuth.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<WebAppsNoAuthDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("WebAppsNoAuthDb")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
    AddCookie(option => { 
        option.LoginPath = "/Access/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });
builder.Services.Configure<Email>(builder.Configuration.GetSection("EmailConfiguration"));
        

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var webAppsNoAuthContext = scope.ServiceProvider.GetRequiredService<WebAppsNoAuthDbContext>();
        webAppsNoAuthContext.Database.EnsureCreated();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();

