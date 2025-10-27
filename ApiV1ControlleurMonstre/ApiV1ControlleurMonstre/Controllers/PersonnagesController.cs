using ApiV1ControlleurMonstre.Constants;
using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonnagesController : Controller
    {
        private readonly MonsterContext _context;

        public PersonnagesController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet("GetPersonnageFromUser/")]
        public async Task<ActionResult<Personnage>> GetPersonnageFromUser()
        {
            try
            {
                Request.Headers.TryGetValue("userToken", out var token);
                Utilisateur? user = await _context.Utilisateurs.FirstOrDefaultAsync(user => user.Token == token.ToString());
                if (user is not null)
                {
                    var personnage = await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == user.Id);
                    if (personnage == null)
                    {
                        return NotFound("Personnage non trouvé pour cet utilisateur");
                    }
                    return personnage;
                }
                else return Unauthorized("InvalidToken: Token is invalid or missing");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur de base de données: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur inattendue: {ex.Message}");
            }
        }


        [HttpPut("MovePersonnage/{direction}")]
        public async Task<IActionResult> MovePersonnage(string direction)
        {
            try
            {
                var authResult = await AuthenticateUser();
                if (authResult.user == null) return Unauthorized("InvalidToken: Token is invalid or missing");

                var personnage = await GetPersonnageByUserId(authResult.user.Id);
                if (personnage == null) return NotFound();

                var (newX, newY) = CalculateNewPosition(personnage, direction);
                if (newX == -1 && newY == -1) return BadRequest("Invalid direction. Use 'up', 'down', 'left', or 'right'.");

                var destinationTuile = await _context.Tuiles.FindAsync(newX, newY);
                if (destinationTuile == null) return BadRequest("La tuile de destination n'existe pas");

                var monstre = await GetMonsterAtPosition(newX, newY);

                if (!destinationTuile.EstTraversable)
                {
                    return BadRequest("Cette tuile n'est pas traversable");
                }
                else if (monstre != null)
                {
                    return await HandleCombat(personnage, monstre, destinationTuile, newX, newY);
                }
                else
                {
                    return await MovePlayerToPosition(personnage, newX, newY);
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur de base de données lors du mouvement: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Opération invalide: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur inattendue lors du mouvement: {ex.Message}");
            }
        }

        private async Task<(Utilisateur? user, StringValues token)> AuthenticateUser()
        {
            try
            {
                Request.Headers.TryGetValue("userToken", out var token);
                var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
                return (user, token);
            }
            catch (Exception ex)
            {
                // Log l'erreur mais ne pas exposer les détails au client
                Console.WriteLine($"Erreur d'authentification: {ex.Message}");
                return (null, default);
            }
        }

        private async Task<Personnage?> GetPersonnageByUserId(int userId)
        {
            try
            {
                return await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du personnage: {ex.Message}");
                return null;
            }
        }

        private (int newX, int newY) CalculateNewPosition(Personnage personnage, string direction)
        {
            int newX = personnage.PositionX;
            int newY = personnage.PositionY;

            switch (direction.ToLower())
            {
                case "up":
                    if (personnage.PositionY > GameConstants.MAP_MIN_POSITION) newY = personnage.PositionY - 1;
                    break;
                case "down":
                    if (personnage.PositionY < GameConstants.MAP_MAX_POSITION) newY = personnage.PositionY + 1;
                    break;
                case "left":
                    if (personnage.PositionX > GameConstants.MAP_MIN_POSITION) newX = personnage.PositionX - 1;
                    break;
                case "right":
                    if (personnage.PositionX < GameConstants.MAP_MAX_POSITION) newX = personnage.PositionX + 1;
                    break;
                default:
                    return (-1, -1); // Valeur d'erreur
            }

            return (newX, newY);
        }

        private async Task<InstanceMonstre?> GetMonsterAtPosition(int x, int y)
        {
            try
            {
                return await _context.InstanceMonstres
                    .Include(m => m.Monstre)
                    .FirstOrDefaultAsync(m => m.PositionX == x && m.PositionY == y);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération du monstre: {ex.Message}");
                return null;
            }
        }

        private async Task<IActionResult> HandleCombat(Personnage personnage, InstanceMonstre monstre, Tuile destinationTuile, int newX, int newY)
        {
            try
            {
                var combatResult = CalculateCombat(personnage, monstre);

                // Application des dégâts
                monstre.PointsVieActuels -= combatResult.degatsAuMonstre;
                personnage.PointsVie -= combatResult.degatsAuJoueur;

                // Sauvegarder les changements même en cas de combat indécis
                await _context.SaveChangesAsync();

                if (monstre.PointsVieActuels <= 0)
                {
                    return await HandlePlayerVictory(personnage, monstre, destinationTuile, newX, newY);
                }
                else if (personnage.PointsVie <= 0)
                {
                    return await HandlePlayerDefeat(personnage);
                }
                else
                {
                    return Ok(new
                    {
                        message = "Indecis",
                        degatsInfliges = combatResult.degatsAuMonstre,
                        degatsRecus = combatResult.degatsAuJoueur,
                        ptsVieJoueur = personnage.PointsVie,
                        ptsVieMonstre = monstre.PointsVieActuels
                    });
                }
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur de base de données lors du combat: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Erreur dans les calculs de combat: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur inattendue lors du combat: {ex.Message}");
            }
        }

        private (int degatsAuMonstre, int degatsAuJoueur) CalculateCombat(Personnage personnage, InstanceMonstre monstre)
        {
            try
            {
                if (personnage == null) throw new ArgumentNullException(nameof(personnage));
                if (monstre?.Monstre == null) throw new ArgumentNullException(nameof(monstre));

                double facteurAleatoire = new Random().NextDouble() * (GameConstants.MAX_COMBAT_FACTOR - GameConstants.MIN_COMBAT_FACTOR) + GameConstants.MIN_COMBAT_FACTOR;

                int degatsAuMonstre = Math.Max(GameConstants.MIN_DAMAGE, (int)((personnage.Force - (monstre.Monstre.DefenseBase + monstre.Niveau)) * facteurAleatoire));
                int degatsAuJoueur = Math.Max(GameConstants.MIN_DAMAGE, (int)((monstre.Monstre.ForceBase + monstre.Niveau - personnage.Defense) * facteurAleatoire));

                return (degatsAuMonstre, degatsAuJoueur);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du calcul de combat: {ex.Message}");
                // Retourner des valeurs par défaut sécuritaires
                return (GameConstants.MIN_DAMAGE, GameConstants.MIN_DAMAGE);
            }
        }

        private async Task<IActionResult> HandlePlayerVictory(Personnage personnage, InstanceMonstre monstre, Tuile destinationTuile, int newX, int newY)
        {
            try
            {
                // Gain d'expérience
                int xpGagnee = monstre.Monstre.ExperienceBase + (monstre.Niveau * GameConstants.BASE_MONSTER_XP_MULTIPLIER);
                personnage.Experience += xpGagnee;

                // Vérification du niveau
                HandleLevelUp(personnage);

                // Supprimer le monstre
                _context.InstanceMonstres.Remove(monstre);

                // Déplacer le joueur
                personnage.PositionX = newX;
                personnage.PositionY = newY;

                await SavePersonnageChanges(personnage);
                return Ok(new
                {
                    message = "WonFight",
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur de base de données lors de la victoire: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur inattendue lors de la victoire: {ex.Message}");
            }
        }

        private void HandleLevelUp(Personnage personnage)
        {
            while (personnage.Experience >= GameConstants.EXPERIENCE_PER_LEVEL)
            {
                personnage.Experience -= GameConstants.EXPERIENCE_PER_LEVEL;
                personnage.Niveau++;
                personnage.Force += GameConstants.STAT_INCREASE_PER_LEVEL;
                personnage.Defense += GameConstants.STAT_INCREASE_PER_LEVEL;
                personnage.PointsVieMax += GameConstants.HP_INCREASE_PER_LEVEL;
                personnage.PointsVie = personnage.PointsVieMax;
            }
        }

        private async Task<IActionResult> HandlePlayerDefeat(Personnage personnage)
        {
            try
            {
                // Trouver une ville proche pour la téléportation
                var villeProche = await _context.Tuiles
                    .Where(t => t.Type == TuileTypeEnum.Ville)
                    .OrderBy(t => Math.Pow(t.PositionX - personnage.PositionX, 2) +
                                Math.Pow(t.PositionY - personnage.PositionY, 2))
                    .FirstOrDefaultAsync();

                if (villeProche != null)
                {
                    personnage.PositionX = villeProche.PositionX;
                    personnage.PositionY = villeProche.PositionY;
                }

                // Restaurer les points de vie
                personnage.PointsVie = personnage.PointsVieMax;

                await SavePersonnageChanges(personnage);
                return Ok(new { message = "LostFight" });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"Erreur de base de données lors de la défaite: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur inattendue lors de la défaite: {ex.Message}");
            }
        }

        private async Task<IActionResult> MovePlayerToPosition(Personnage personnage, int newX, int newY)
        {
            try
            {
                personnage.PositionX = newX;
                personnage.PositionY = newY;

                await SavePersonnageChanges(personnage);
                return Ok(new { message = "Moved" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erreur lors du déplacement du personnage: {ex.Message}");
            }
        }

        private async Task SavePersonnageChanges(Personnage personnage)
        {
            _context.Entry(personnage).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonnageExists(personnage.UtilisateurID))
                {
                    throw new InvalidOperationException("Personnage not found");
                }
                else
                {
                    throw;
                }
            }
        }


        private bool PersonnageExists(int id)
        {
            try
            {
                return _context.Personnages.Any(e => e.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification d'existence du personnage: {ex.Message}");
                return false;
            }
        }
    }
}
