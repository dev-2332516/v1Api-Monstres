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

        [HttpGet("GetPersonnageFromUser/{userID}")]
        public async Task<ActionResult<Personnage>> GetPersonnageFromUser(int userID)
        {
            var personnage = await _context.Personnages
                .FirstOrDefaultAsync(p => p.UtilisateurID == userID);
            if (personnage == null)
            {
                return NotFound();
            }
            return personnage;
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

        [HttpPut("MovePersonnage/{id}/{newX}/{newY}")]
        public async Task<IActionResult> MovePersonnage(int id, int newX, int newY)
        {
            Request.Headers.TryGetValue("userToken", out StringValues token);
            if (_context.Utilisateurs.ToListAsync().Result.Where(user => user.Token == token).FirstOrDefault() is not null)
            {

                var personnage = await _context.Personnages.FindAsync(id);
                if (personnage == null)
                {
                    return NotFound();
                }
                // Check si le personnage est dans les bounds
                if ((newX < 0 || newX > 100 || newY < 0 || newY > 100) && (newX < 2 || newX > 98 || newY < 2 || newY > 98))
                {
                    return BadRequest("New position is out of bounds.");
                }
                personnage.PositionX = newX;
                personnage.PositionY = newY;
                _context.Entry(personnage).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonnageExists(id))
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
