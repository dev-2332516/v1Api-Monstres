using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.DTOs;
using ApiV1ControlleurMonstre.Models;
using ApiV1ControlleurMonstre.Utilities;
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

        // GET: api/GetOrCreateTuile/10/10
        [HttpGet("GetOrCreateTuile/{positionX}/{positionY}")]
        public async Task<ActionResult<TuileDto>> GetOrCreateTuile(int positionX, int positionY)
        {
            Request.Headers.TryGetValue("userToken", out StringValues token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
            if (user == null) return Unauthorized("InvalidToken");

            var tuile = await _context.Tuiles.FindAsync(positionX, positionY);

            if (tuile == null)
            {
                await PostTuile(TileGenerator.GenerateTuile(positionX, positionY));
                tuile = await _context.Tuiles.FindAsync(positionX, positionY);
            }

            var monstre = await _context.InstanceMonstres
                .Include(im => im.Monstre)
                .FirstOrDefaultAsync(m => m.PositionX == positionX && m.PositionY == positionY);

            return TuileDto.FromModel(tuile, monstre);
        }

        // GET: api/Tuiles/5/5
        [HttpGet("GetTuile/{positionX}/{positionY}")]
        public async Task<ActionResult<TuileDto>> GetTuile(int positionX, int positionY)
        {
            Request.Headers.TryGetValue("userToken", out StringValues token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
            if (user == null) return Unauthorized("InvalidToken");

            var tuile = await _context.Tuiles.FindAsync(positionX, positionY);
            if (tuile is null) return null;

            var monstre = await _context.InstanceMonstres
                .Include(im => im.Monstre)
                .FirstOrDefaultAsync(m => m.PositionX == positionX && m.PositionY == positionY);

            return TuileDto.FromModel(tuile, monstre);
        }

        // GET: api/Tuiles/10/10
        // Get une ligne de tuiles à partir d'une orientation
        [HttpGet("GetTuilesLine/{orientation}")]
        public async Task<ActionResult<TuileDto[]>> GetTuilesLine(string orientation)
        {
            Request.Headers.TryGetValue("userToken", out StringValues token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
            if (user == null) return Unauthorized("InvalidToken");
            var personnage = await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == user.Id);
            if (personnage == null) return NotFound("PersonnageNotFound");

            int positionX = personnage.PositionX;
            int positionY = personnage.PositionY;

            // Vérifier si l'orientation est valide
            if (orientation != "up" && orientation != "down" && orientation != "left" && orientation != "right")
                return BadRequest($"InvalidOrientation: Orientation \"{orientation}\" is invalid\n\tValid inputs are: up, down, left, right");
            
            TuileDto[] tuilesArray = new TuileDto[3];
            Tuile tuile = null;

            for (int value = -1; value <= 1; value++)
            {
                switch (orientation)
                {
                    case "up":
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY - 1);
                        if (tuile == null) await PostTuile(TileGenerator.GenerateTuile(positionX + value, positionY - 1));
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY - 1);
                        break;
                    case "down":
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY + 1);
                        if (tuile == null) await PostTuile(TileGenerator.GenerateTuile(positionX + value, positionY + 1));
                        tuile = await _context.Tuiles.FindAsync(positionX + value, positionY + 1);
                        break;
                    case "left":
                        tuile = await _context.Tuiles.FindAsync(positionX - 1, positionY + value);
                        if (tuile == null) await PostTuile(TileGenerator.GenerateTuile(positionX - 1, positionY + value));
                        tuile = await _context.Tuiles.FindAsync(positionX - 1, positionY + value);
                        break;
                    case "right":
                        tuile = await _context.Tuiles.FindAsync(positionX + 1, positionY + value);
                        if (tuile == null) await PostTuile(TileGenerator.GenerateTuile(positionX + 1, positionY + value));
                        tuile = await _context.Tuiles.FindAsync(positionX + 1, positionY + value);
                        break;
                    default:
                        return BadRequest($"InvalidOrienation: Orienation \"{orientation}\" is invalid\nValid inputs are: up, down, left, right");
                }
                if (tuile is null) 
                {
                    tuilesArray[value + 1] = null;
                }
                else 
                {
                    var monstre = await _context.InstanceMonstres
                        .Include(im => im.Monstre)
                        .FirstOrDefaultAsync(m => m.PositionX == tuile.PositionX && m.PositionY == tuile.PositionY);
                    tuilesArray[value + 1] = TuileDto.FromModel(tuile, monstre);
                }
            }
            return tuilesArray;
        }

        // GET: api/Tuiles/GetInitialTuiles
        [HttpGet("GetInitialTuiles")]
        public async Task<ActionResult<List<TuileDto>>> GetInitialTuiles()
        {
            Request.Headers.TryGetValue("userToken", out StringValues token);
            Utilisateur user = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Token == token.ToString());
            if (user == null) return Unauthorized("InvalidToken");
            var personnage = await _context.Personnages.FirstOrDefaultAsync(p => p.UtilisateurID == user.Id);
            if (personnage == null) return NotFound("PersonnageNotFound");
            int positionX = personnage.PositionX;
            int positionY = personnage.PositionY;

            List<TuileDto> tuilesArray = new List<TuileDto>();
            Tuile tuile = null;

            // Prend toutes les tuiles dans la map
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    tuile = await _context.Tuiles.FindAsync(positionX + x, positionY + y);
                    if ((x >= -1 && x <= 1) && (y >= -1 && y <= 1) && tuile is null)
                    {
                        await PostTuile(TileGenerator.GenerateTuile(positionX + x, positionY + y));
                        tuile = await _context.Tuiles.FindAsync(positionX + x, positionY + y);
                    }
                    
                    if (tuile != null)
                    {
                        var monstre = await _context.InstanceMonstres
                            .Include(im => im.Monstre)
                            .FirstOrDefaultAsync(m => m.PositionX == tuile.PositionX && m.PositionY == tuile.PositionY);
                        tuilesArray.Add(TuileDto.FromModel(tuile, monstre));
                    }
                }
            }
            return tuilesArray;
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

            return CreatedAtAction(nameof(GetTuile), new { positionX = tuile.PositionX, positionY = tuile.PositionY }, tuile);
        }

        // DELETE: api/Tuiles/5
        [HttpDelete("{positionX}/{positionY}")]
        public async Task<IActionResult> DeleteTuile(int positionX, int positionY)
        {
            var tuile = await _context.Tuiles.FindAsync(positionX, positionY);
            if (tuile == null)
            {
                return NotFound();
            }

            _context.Tuiles.Remove(tuile);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("GetTuileType/{type}")]
        public ActionResult<string> GetTuileType(int type)
        {
            string value = Enum.GetName(typeof(TuileTypeEnum), type);
            return value;
        }

        private bool TuileExists(int positionX)
        {
            return _context.Tuiles.Any(e => e.PositionX == positionX);
        }
        
    }
}
