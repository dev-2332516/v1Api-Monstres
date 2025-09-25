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

        [HttpPost("CreatePersonnage/{userId}/{name}")]
        public async Task<ActionResult<Personnage>> CreatePersonnage(int userId, string name)
        {
            Random rand = new Random();
            Personnage personnage = new Personnage
            {
                Name = name,
                Niveau = 1,
                Experience = 0,
                PointsVie = 100,
                PointsVieMax = 100,
                Force = rand.Next(10, 21),
                Defense = rand.Next(5, 16),
                PositionX = rand.Next(2, 98),
                PositionY = rand.Next(2, 98),
                UtilisateurID = userId,
                DateCreation = DateTime.Now
            };
            _context.Personnages.Add(personnage);
            _context.SaveChanges();
            return personnage;
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

                switch (direction.ToLower())
                {
                    case "up":
                        if (personnage.PositionY < 98) personnage.PositionY += 1;
                        break;
                    case "down":
                        if (personnage.PositionY > 2) personnage.PositionY -= 1;
                        break;
                    case "left":
                        if (personnage.PositionX > 2) personnage.PositionX -= 1;
                        break;
                    case "right":
                        if (personnage.PositionX < 98) personnage.PositionX += 1;
                        break;
                    default:
                        return BadRequest("Invalid direction. Use 'up', 'down', 'left', or 'right'.");
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
