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

// ⭐ AJOUT : Configuration des sessions
builder.Services.AddDistributedMemoryCache(); // Stockage en mémoire
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Durée de la session
    options.Cookie.HttpOnly = true; // Sécurité contre XSS
    options.Cookie.IsEssential = true; // Nécessaire pour RGPD
    options.Cookie.SameSite = SameSiteMode.Lax; // Protection CSRF
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS uniquement
});

// ⭐ AJOUT : CORS pour Next.js
builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL de ton Next.js
              .AllowCredentials() // Important pour les cookies !
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

app.UseCors("NextJsPolicy"); // ⭐ AJOUT

app.UseSession(); // ⭐ AJOUT : Active les sessions

app.UseAuthorization();

app.MapControllers();

app.Run();