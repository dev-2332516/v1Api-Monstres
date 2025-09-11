using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;

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

        // GET: api/Tuiles/10/10
        [HttpGet("{positionX}/{positionY}")]
        public async Task<ActionResult<Tuile>> GetTuile(int positionX, int positionY)
        {
            var tuile = await _context.Tuiles.FindAsync(positionX,positionY);

            if (tuile == null)
            {
                Random random = new Random();
                int randomNumber = random.Next(1, 101);
                TuileTypeEnum type;
                string imageUrl;
                if (randomNumber <= 20) {
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
                    tuile = new Tuile(positionX, positionY, TuileTypeEnum.Foret, false, Tuile.stringImageUrl[TuileTypeEnum.Foret]);
                }
                else if (randomNumber <= 65)
                {
                    tuile = new Tuile(positionX, positionY, TuileTypeEnum.Ville, false, Tuile.stringImageUrl[TuileTypeEnum.Ville]);
                }
                else
                {
                    tuile = new Tuile(positionX, positionY, TuileTypeEnum.Route, true, Tuile.stringImageUrl[TuileTypeEnum.Route]);
                }

                await PostTuile(tuile);
            }

            return tuile;
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
    }
}
