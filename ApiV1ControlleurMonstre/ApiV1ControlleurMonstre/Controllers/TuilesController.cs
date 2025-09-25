using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TuilesController : ControllerBase
    {
        private readonly MonsterContext _context;

        public TuilesController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/Tuiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tuile>>> GetTuiles()
        {
            return await _context.Tuiles.ToListAsync();
        }

        // GET: api/GetOrCreateTuile/10/10
        [HttpGet("GetOrCreateTuile/{positionX}/{positionY}")]
        public async Task<ActionResult<Tuile>> GetOrCreateTuile(int positionX, int positionY)
        {
            var tuile = await _context.Tuiles.FindAsync(positionX, positionY);

            if (tuile == null)
            {
                await PostTuile(GenerateTuile(positionX, positionY));
                tuile = await _context.Tuiles.FindAsync(positionX, positionY);
            }
                return tuile;
        }

        // GET: api/GetTuile/10/10
        [HttpGet("GetTuile/{positionX}/{positionY}")]
        public async Task<ActionResult<Tuile>> GetTuile(int positionX, int positionY)
        {
            var tuile = await _context.Tuiles.FindAsync(positionX, positionY);
            if (tuile is null) return null;
            return tuile;
        }

        // GET: api/Tuiles/10/10
        // Get une ligne de tuiles à partir d'une orientation
        [HttpGet("GetTuilesLine/{positionX}/{positionY}/{orientation}")]
        public async Task<ActionResult<Tuile[]>> GetTuilesLine(int positionX, int positionY, string orientation)
        {
            HttpContext.Request.Headers.TryGetValue("userToken", out StringValues headerValue);
            string first = headerValue.FirstOrDefault();
            // Verifier si l'orientation est valide
            if (orientation != "up" && orientation != "down" && orientation != "left" && orientation != "right")
                return BadRequest($"InvalidOrienation: Orienation \"{orientation}\" is invalid\n\tValid inputs are: up, down, left, right");
            Tuile[] tuilesArray = new Tuile[3];
            Tuile tuile = null;

            for (int value = -1; value <= 1; value++)
            {
                switch (orientation)
                {
                    case "up":
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY - 1);
                        if (tuile == null) await PostTuile(GenerateTuile(positionX + value, positionY - 1));
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY - 1);
                        break;
                    case "down":
                        tuile = await _context.Tuiles.FindAsync(positionX - value, positionY + 2);
                        if (tuile == null) await PostTuile(GenerateTuile(positionX - value, positionY + 1));
                        tuile = await _context.Tuiles.FindAsync(positionX - value, positionY + 1);
                        break;
                    case "left":
                        tuile = await _context.Tuiles.FindAsync(positionX - 2, positionY + value);
                        if (tuile == null) await PostTuile(GenerateTuile(positionX - 2, positionY + value));
                        tuile = await _context.Tuiles.FindAsync(positionX - value, positionY + value);
                        break;
                    case "right":
                        tuile = await _context.Tuiles.FindAsync(positionX + 2, positionY - value);
                        if (tuile == null) await PostTuile(GenerateTuile(positionX + 2, positionY - value));
                        tuile = await _context.Tuiles.FindAsync(positionX + 2, positionY -value);
                        break;
                    default:
                        return BadRequest($"InvalidOrienation: Orienation \"{orientation}\" is invalid\nValid inputs are: up, down, left, right");
                }
                if (tuile is null) tuilesArray[value + 1] = null;
                else tuilesArray[value + 1] = tuile;
            }
            return tuilesArray;
        }

        //GET: api/Tuiles/10/10
        [HttpGet("GetInitialTuiles/{positionX}/{positionY}")]
        public async Task<ActionResult<List<Tuile>>> GetInitialTuiles(int positionX, int positionY)
        {
            List<Tuile> tuilesArray = new List<Tuile>();
            Tuile tuile = null;

            // Prend toutes les tuiles dans la map
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    tuile = await _context.Tuiles.FindAsync(positionX + x, positionY + y);
                    if ((x >= -1 && x <= 1) && (y >= -1 && y <= 1) && tuile is null)
                    {
                        await PostTuile(GenerateTuile(positionX + x, positionY + y));
                        tuile = await _context.Tuiles.FindAsync(positionX + x, positionY + y);
                    }
                    tuilesArray.Add(tuile);
                }
            }
            return tuilesArray;
        }

        // PUT: api/Tuiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTuile(int id, Tuile tuile)
        {
            if (id != tuile.PositionX)
            {
                return BadRequest();
            }

            _context.Entry(tuile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TuileExists(id))
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

        // POST: api/Tuiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Tuile>> PostTuile(Tuile tuile)
        {
            _context.Tuiles.Add(tuile);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TuileExists(tuile.PositionX))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetTuile", new { id = tuile.PositionX }, tuile);
        }

        // DELETE: api/Tuiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTuile(int id)
        {
            var tuile = await _context.Tuiles.FindAsync(id);
            if (tuile == null)
            {
                return NotFound();
            }

            _context.Tuiles.Remove(tuile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TuileExists(int id)
        {
            return _context.Tuiles.Any(e => e.PositionX == id);
        }

        private Tuile GenerateTuile(int positionX, int positionY)
        {
            Tuile tuile;
            Random random = new Random();
            int randomNumber = random.Next(1, 101);
            TuileTypeEnum type;
            string imageUrl;
            if (randomNumber <= 20)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Herbe, true, Tuile.stringImageUrl[TuileTypeEnum.Herbe]);
            }
            else if (randomNumber <= 30)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Eau, false, Tuile.stringImageUrl[TuileTypeEnum.Eau]);
            }
            else if (randomNumber <= 45)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Montagne, false, Tuile.stringImageUrl[TuileTypeEnum.Montagne]);
            }
            else if (randomNumber <= 60)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Foret, true, Tuile.stringImageUrl[TuileTypeEnum.Foret]);
            }
            else if (randomNumber <= 65)
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Ville, true, Tuile.stringImageUrl[TuileTypeEnum.Ville]);
            }
            else
            {
                tuile = new Tuile(positionX, positionY, TuileTypeEnum.Route, true, Tuile.stringImageUrl[TuileTypeEnum.Route]);
            }
            return tuile;
        }
    }
}
