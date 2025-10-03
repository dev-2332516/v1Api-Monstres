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
            Request.Headers.TryGetValue("userToken", out var token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(user => user.Token == token.ToString());
            if (user is not null)
            {
                var personnage = await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == user.Id);
                if (personnage == null)
                {
                    return NotFound();
                }

                int newX = personnage.PositionX;
                int newY = personnage.PositionY;

                switch (direction.ToLower())
                {
                    case "up":
                        if (personnage.PositionY > 2) newY = personnage.PositionY - 1;
                        break;
                    case "down":
                        if (personnage.PositionY < 48) newY = personnage.PositionY + 1;
                        break;
                    case "left":
                        if (personnage.PositionX > 2) newX = personnage.PositionX - 1;
                        break;
                    case "right":
                        if (personnage.PositionX < 48) newX = personnage.PositionX + 1;
                        break;
                    default:
                        return BadRequest("Invalid direction. Use 'up', 'down', 'left', or 'right'.");
                }

                // Vérifier si la tuile de destination est traversable
                var destinationTuile = await _context.Tuiles.FindAsync(newX, newY);
                if (destinationTuile == null)
                {
                    return BadRequest("La tuile de destination n'existe pas");
                }

                // Vérifier s'il y a un monstre sur la tuile
                var monstre = await _context.InstanceMonstres
                    .Include(m => m.Monstre)
                    .FirstOrDefaultAsync(m => m.PositionX == newX && m.PositionY == newY);

                if (monstre != null)
                {
                    // Combat !
                    double facteurAleatoire = new Random().NextDouble() * (1.25 - 0.8) + 0.8;
                    
                    // Calcul des dégâts avec un minimum de 1
                    int degatsAuMonstre = Math.Max(1, (int)((personnage.Force - (monstre.Monstre.DefenseBase + monstre.Niveau)) * facteurAleatoire));
                    int degatsAuJoueur = Math.Max(1, (int)((monstre.Monstre.ForceBase + monstre.Niveau - personnage.Defense) * facteurAleatoire));

                    // Application des dégâts
                    monstre.PointsVieActuels -= degatsAuMonstre;
                    personnage.PointsVie -= degatsAuJoueur;
                    
                    // Sauvegarder les changements même en cas de combat indécis
                    await _context.SaveChangesAsync();

                    if (monstre.PointsVieActuels <= 0)
                    {
                        // Victoire du joueur
                        // Gain d'expérience
                        int xpGagnee = monstre.Monstre.ExperienceBase + (monstre.Niveau * 10);
                        personnage.Experience += xpGagnee;

                        // Vérification du niveau
                        while (personnage.Experience >= 100)
                        {
                            personnage.Experience -= 100;
                            personnage.Niveau++;
                            personnage.Force++;
                            personnage.Defense++;
                            personnage.PointsVieMax++;
                            personnage.PointsVie = personnage.PointsVieMax;
                        }

                        // Supprimer le monstre
                        _context.InstanceMonstres.Remove(monstre);

                        // Déplacer le joueur si la tuile est traversable
                        if (destinationTuile.EstTraversable)
                        {
                            personnage.PositionX = newX;
                            personnage.PositionY = newY;
                        }
                    }
                    else if (personnage.PointsVie <= 0)
                    {
                        // Défaite du joueur
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
                        
                        return Ok(new { message = "Vous avez été vaincu et téléporté à la ville la plus proche!" });
                    }
                    else
                    {
                        // Combat indécis
                        return Ok(new { 
                            message = "Combat ! ",
                            degatsInfliges = degatsAuMonstre,
                            degatsRecus = degatsAuJoueur,
                            ptsVieJoueur = personnage.PointsVie,
                            ptsVieMonstre = monstre.PointsVieActuels
                        });
                    }
                }
                else if (!destinationTuile.EstTraversable)
                {
                    return BadRequest("Cette tuile n'est pas traversable");
                }
                else
                {
                    // Déplacement normal sans combat
                    personnage.PositionX = newX;
                    personnage.PositionY = newY;
                }

                _context.Entry(personnage).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonnageExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok();
            }
            else return Unauthorized("InvalidToken: Token is invalid or missing");
        }


        private bool PersonnageExists(int id)
        {
            return _context.Personnages.Any(e => e.Id == id);
        }
    }
}
