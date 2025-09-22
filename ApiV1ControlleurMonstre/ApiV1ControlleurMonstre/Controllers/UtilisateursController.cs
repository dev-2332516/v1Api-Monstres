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
        public async Task<ActionResult<Utilisateur>> Register([FromBody] Utilisateur utilisateur)
        {
            if (UtilisateurExist(utilisateur.Email))
                return Conflict("EmailAlreadyExists");

            utilisateur.Password = Hashing.Compute(utilisateur.Password);
            utilisateur.DateInscription = DateTime.Now;
            utilisateur.IsConnected = false;

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Créer un personnage par défaut lié à ce nouvel utilisateur
            var personnage = new Personnage
                {
                    UtilisateurID = utilisateur.Id,
                    Name = $"Hero_{utilisateur.Id}",
                    Niveau = 1,
                    Experience = 0,
                    PointsVie = 100,           // valeur de départ, ajustable
                    PointsVieMax = 100,        // valeur maximale, ajustable
                    Force = 10,                // valeur de base, ajustable
                    Defense = 5,               // valeur de base, ajustable
                    PositionX = 0,
                    PositionY = 0,
                    DateCreation = DateTime.Now
                };

            _context.Personnages.Add(personnage);
            await _context.SaveChangesAsync();

            return utilisateur;
        }

        [HttpPost("login/{email}/{password}")]
        public async Task<ActionResult<string>> Login(string email, string password)
        {
            // Vérification de l'utilisateur
            var utilisateur = await _context.Utilisateurs
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == Hashing.Compute(password));

            if (utilisateur == null)
                return Unauthorized("InvalidEmailPassword");

            var token = GenerateJwtToken(utilisateur);
            return Ok(token);
        }


        [HttpPost("logout/{id}")]
        public async Task<ActionResult> Logout(int id)
        {
            var utilisateur = await _context.Utilisateurs.FindAsync(id);
            if (utilisateur == null) return NotFound("InvalidID");

            utilisateur.IsConnected = false;
            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool UtilisateurExist(string email)
        {
            return _context.Utilisateurs.Any(u => u.Email == email);
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
