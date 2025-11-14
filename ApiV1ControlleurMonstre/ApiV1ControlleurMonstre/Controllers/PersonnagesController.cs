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
            Request.Headers.TryGetValue("userToken", out var token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(user => user.Token == token.ToString());
            if (user is not null)
            {
                var personnage = await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == user.Id);
                if (personnage == null)
                {
                    return NotFound();
                }
                return personnage;
            }
            else return Unauthorized("InvalidToken: Token is invalid or missing");
        }


        [HttpPut("MovePersonnage/{direction}")]
        public async Task<IActionResult> MovePersonnage(string direction)
        {
            var authResult = await AuthenticateUser();
            if (authResult.user == null) return Unauthorized("InvalidToken: Token is invalid or missing");

            var personnage = await GetPersonnageByUserId(authResult.user.Id);
            if (personnage == null) return NotFound();

            var (newX, newY) = CalculateNewPosition(personnage, direction);
            if (newX == -1 && newY == -1) return BadRequest("Invalid direction. Use 'up', 'down', 'left', or 'right'.");

            var destinationTuile = await _context.Tuiles.FindAsync(newX, newY);
            if (destinationTuile == null) return BadRequest("La tuile de destination n'existe pas");

            #region Logique de quetes
            List<Quete> listQuest = GetQuestByType(TypeQuete.Destination, personnage).Result;
            if (listQuest.Any())
            {
                foreach (Quete quete in listQuest)
                {
                    if (quete.Destination == destinationTuile)
                    {
                        // Modifie la quete
                        _context.Remove(quete);
                        quete.EstCompleter = true;
                        _context.Add(quete);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            #endregion

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

        private async Task<(Utilisateur user, StringValues token)> AuthenticateUser()
        {
            Request.Headers.TryGetValue("userToken", out var token);
            var user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
            return (user, token);
        }

        private async Task<Personnage> GetPersonnageByUserId(int userId)
        {
            return await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == userId);
        }

        private (int newX, int newY) CalculateNewPosition(Personnage personnage, string direction)
        {
            int newX = personnage.PositionX;
            int newY = personnage.PositionY;

            switch (direction.ToLower())
            {
                case "up":
                    newY = personnage.PositionY - 1;
                    break;
                case "down":
                    newY = personnage.PositionY + 1;
                    break;
                case "left":
                    newX = personnage.PositionX - 1;
                    break;
                case "right":
                    newX = personnage.PositionX + 1;
                    break;
                default:
                    return (-1, -1); // Valeur d'erreur
            }

            // Vérification finale des limites de la carte (0 à 50)
            if (newX < 0 || newX > 50 || newY < 0 || newY > 50)
            {
                return (-1, -1); // Mouvement invalide
            }

            return (newX, newY);
        }

        private async Task<InstanceMonstre> GetMonsterAtPosition(int x, int y)
        {
            return await _context.InstanceMonstres
                .Include(m => m.Monstre)
                .FirstOrDefaultAsync(m => m.PositionX == x && m.PositionY == y);
        }

        private async Task<IActionResult> HandleCombat(Personnage personnage, InstanceMonstre monstre, Tuile destinationTuile, int newX, int newY)
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

        private (int degatsAuMonstre, int degatsAuJoueur) CalculateCombat(Personnage personnage, InstanceMonstre monstre)
        {
            double facteurAleatoire = new Random().NextDouble() * (1.25 - 0.8) + 0.8;

            int degatsAuMonstre = Math.Max(1, (int)((personnage.Force - (monstre.Monstre.DefenseBase + monstre.Niveau)) * facteurAleatoire));
            int degatsAuJoueur = Math.Max(1, (int)((monstre.Monstre.ForceBase + monstre.Niveau - personnage.Defense) * facteurAleatoire));

            return (degatsAuMonstre, degatsAuJoueur);
        }

        private async Task<IActionResult> HandlePlayerVictory(Personnage personnage, InstanceMonstre monstre, Tuile destinationTuile, int newX, int newY)
        {
            // Gain d'expérience
            int xpGagnee = monstre.Monstre.ExperienceBase + (monstre.Niveau * 10);
            personnage.Experience += xpGagnee;

            List<Quete> listQuest = GetQuestByType(TypeQuete.Monstre, personnage).Result;
            if (listQuest.Any())
            {
                foreach (Quete quete in listQuest)
                {
                    if (quete.TypeMonstre == monstre.Monstre.Type1 || quete.TypeMonstre == monstre.Monstre.Type2)
                    {
                        // Modifie la quete
                        _context.Remove(quete);

                        quete.NombreActuellementTuer++;
                        if (quete.NombreATuer == quete.NombreActuellementTuer) quete.EstCompleter = true;

                        _context.Add(quete);
                        await _context.SaveChangesAsync();
                    }
                }
            }

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

        private async void HandleLevelUp(Personnage personnage)
        {
            while (personnage.Experience >= 100)
            {
                personnage.Experience -= 100;
                personnage.Niveau++;
                personnage.Force++;
                personnage.Defense++;
                personnage.PointsVieMax++;
                personnage.PointsVie = personnage.PointsVieMax;
            }

            List<Quete> listQuest = GetQuestByType(TypeQuete.Niveau, personnage).Result;
            if (listQuest.Any())
            {
                foreach (Quete quete in listQuest)
                {
                    if (quete.NiveauSauvegarder + 1 == personnage.Niveau)
                    {
                        _context.Remove(quete);
                        quete.EstCompleter = true;
                        _context.Add(quete);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        private async Task<IActionResult> HandlePlayerDefeat(Personnage personnage)
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

        private async Task<IActionResult> MovePlayerToPosition(Personnage personnage, int newX, int newY)
        {
            personnage.PositionX = newX;
            personnage.PositionY = newY;
            await SavePersonnageChanges(personnage);
            return Ok(new { message = "Moved" });
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
            return _context.Personnages.Any(e => e.Id == id);
        }

        private async Task<List<Quete>> GetQuestByType(TypeQuete type, Personnage personnage)
        {
            return await _context.Quetes.Where(quete =>
               quete.Personnage == personnage &&
               quete.TypeQuete == TypeQuete.Monstre)
               .ToListAsync();
        }
    }
}
