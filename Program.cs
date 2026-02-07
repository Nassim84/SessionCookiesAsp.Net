using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using MonBackendAspNet.Data;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la connexion SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ⭐ Configuration des sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Pour dev local, mettre Always en prod
    options.Cookie.Name = ".MonApp.Session";
});

// ⭐ CORS pour Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowCredentials()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Mon API",
        Version = "v1",
        Description = "Une API de test avec ASP.NET Core et Swagger"
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mon API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// ⭐ Ordre important !
app.UseCors("NextJsPolicy");
app.UseSession();
app.UseAuthorization();

app.MapControllers();

app.Run();