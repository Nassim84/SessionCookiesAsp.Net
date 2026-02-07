using Microsoft.AspNetCore.Mvc;
using MonBackendAspNet.Models;
using MonBackendAspNet.Data;
using MonBackendAspNet.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MonBackendAspNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Un utilisateur avec cet email existe déjà." });

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
            };

            var hasher = new PasswordHasher<User>();
            user.Password = hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Compte créé avec succès." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Identifiants invalides." });

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Identifiants invalides." });

            // ⭐ Créer la session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Email", user.Email);

            return Ok(new
            {
                message = "Connexion réussie",
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email
                }
            });
        }

        // ⭐ Vérifier si l'utilisateur est connecté
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized(new { message = "Non authentifié" });

            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");

            return Ok(new
            {
                id = userId,
                username = username,
                email = email
            });
        }

        // ⭐ Déconnexion
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Déconnexion réussie" });
        }
    }
}