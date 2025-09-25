using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateursController : Controller
    {
        private readonly MonsterContext _context;
        private readonly JwtSettings _jwtSettings;

        public UtilisateursController(MonsterContext context, IOptions<JwtSettings> options)
        {
            _context = context;
            _jwtSettings = options.Value;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Utilisateur utilisateur)
        {
            if (await UtilisateurExist(utilisateur.Email))
                return Conflict("EmailAlreadyExists");

            utilisateur.Password = Hashing.Compute(utilisateur.Password);
            utilisateur.DateInscription = DateTime.Now;
            utilisateur.IsConnected = true;
            utilisateur.Token = GenerateJwtToken(utilisateur);

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Créer un personnage par défaut lié à ce nouvel utilisateur
            Random rand = new Random();
            var personnage = new Personnage
                {
                    UtilisateurID = utilisateur.Id,
                    Name = utilisateur.Pseudo,
                    Niveau = 1,
                    Experience = 0,
                    PointsVie = 100,           // valeur de départ, ajustable
                    PointsVieMax = 100,        // valeur maximale, ajustable
                    Force = 10,                // valeur de base, ajustable
                    Defense = 5,               // valeur de base, ajustable
                    PositionX = rand.Next(2, 98), // Position aléatoire dans les limites valides
                    PositionY = rand.Next(2, 98), // Position aléatoire dans les limites valides
                    DateCreation = DateTime.Now
                };

            _context.Personnages.Add(personnage);
            await _context.SaveChangesAsync();

            return Ok(new { token = utilisateur.Token });
        }

        [HttpPost("login/{email}/{password}")]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Vérification de l'utilisateur
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == Hashing.Compute(password));

            if (utilisateur == null)
                return Unauthorized("InvalidEmailPassword");

            utilisateur.IsConnected = true;
            await _context.SaveChangesAsync();

            return Ok(new {utilisateur.Token });
        }


        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            Request.Headers.TryGetValue("userToken", out var token);
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Token == token.ToString());
            
            if (utilisateur == null) return NotFound("InvalidToken");

            utilisateur.IsConnected = false;

            await _context.SaveChangesAsync();
            utilisateur = null;

            return Ok("Déconnexion réussie");
        }

        private async Task<bool> UtilisateurExist(string email)
        {
            return await _context.Utilisateurs.AnyAsync(u => u.Email == email);
        }

        private string GenerateJwtToken(Utilisateur utilisateur)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                    new Claim(ClaimTypes.Email, utilisateur.Email),
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
