using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Constants;
using ApiV1ControlleurMonstre.Models;
using ApiV1ControlleurMonstre.Utilities;
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

            utilisateur.Password = HashingUtility.Compute(utilisateur.Password);
            utilisateur.DateInscription = DateTime.Now;
            utilisateur.IsConnected = true;
            utilisateur.Token = GenerateJwtToken(utilisateur);

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            // Créer un personnage par défaut lié à ce nouvel utilisateur
            Random rand = new Random();
            int posX, posY;
            Tuile spawnTuile;

            // Chercher une tuile traversable pour le spawn
            do
            {
                posX = rand.Next(2, 48);
                posY = rand.Next(2, 48);
                spawnTuile = await _context.Tuiles.FindAsync(posX, posY);
                
                // Si la tuile n'existe pas, la créer
                if (spawnTuile == null)
                {
                    var tuilesController = new TuilesController(_context);
                    spawnTuile = TileGenerator.GenerateTuile(posX, posY);
                    
                    // Ne sauvegarder la tuile que si elle est traversable
                    if (spawnTuile.EstTraversable)
                    {
                        await _context.Tuiles.AddAsync(spawnTuile);
                        await _context.SaveChangesAsync();
                    }
                }
            } while (spawnTuile == null || !spawnTuile.EstTraversable);

            var personnage = new Personnage
            {
                UtilisateurID = utilisateur.Id,
                Name = utilisateur.Pseudo,
                Niveau = GameConstants.DEFAULT_PLAYER_LEVEL,
                Experience = 0,
                PointsVie = GameConstants.DEFAULT_PLAYER_HP,
                PointsVieMax = GameConstants.DEFAULT_PLAYER_MAX_HP,
                Force = GameConstants.DEFAULT_PLAYER_FORCE,
                Defense = GameConstants.DEFAULT_PLAYER_DEFENSE,
                PositionX = posX,
                PositionY = posY,
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
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == HashingUtility.Compute(password));

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
                Expires = DateTime.UtcNow.AddHours(GameConstants.JWT_EXPIRY_HOURS),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
