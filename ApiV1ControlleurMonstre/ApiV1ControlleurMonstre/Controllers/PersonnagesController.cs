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
                        if (personnage.PositionY < 98) newY = personnage.PositionY + 1;
                        break;
                    case "down":
                        if (personnage.PositionY > 2) newY = personnage.PositionY - 1;
                        break;
                    case "left":
                        if (personnage.PositionX > 2) newX = personnage.PositionX - 1;
                        break;
                    case "right":
                        if (personnage.PositionX < 98) newX = personnage.PositionX + 1;
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
                if (!destinationTuile.EstTraversable)
                {
                    return BadRequest("Cette tuile n'est pas traversable");
                }

                // Si la tuile est traversable, mettre à jour la position
                personnage.PositionX = newX;
                personnage.PositionY = newY;

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
                return NoContent();
            }
            else return Unauthorized("InvalidToken: Token is invalid or missing");
        }


        private bool PersonnageExists(int id)
        {
            return _context.Personnages.Any(e => e.Id == id);
        }
    }
}
