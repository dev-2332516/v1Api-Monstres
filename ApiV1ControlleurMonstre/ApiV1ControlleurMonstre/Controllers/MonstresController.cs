using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Controllers
{
    public class MonstresController : Controller
    {
        private readonly MonsterContext _context;

        public MonstresController(MonsterContext context)
        {
            _context = context;
        }

        //GET: api/Monstre/id/5
        [HttpGet("Id/{id}")]
        public async Task<ActionResult<Monstre>> GetMonstreByID(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var monstre = await _context.Monstre
                .FirstOrDefaultAsync(m => m.Id == id);
            if (monstre == null)
            {
                return NotFound();
            }

            return monstre;
        }
        // GET: api/name/bulbasaur
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Monstre>> GetMonstreByName(string name)
        {
            if (name == null)
            {
                return NotFound();
            }

            var monstre = await _context.Monstre
                .FirstOrDefaultAsync(m => m.Nom == name);
            if (monstre == null)
            {
                return NotFound();
            }

            return monstre;
        }

        // GET: api/PVBase/45
        [HttpGet("PVBase/{pvBase}")]
        public async Task<ActionResult<IEnumerable<Monstre>>> GetMonstreByPVBase(int pvBase)
        {
            return await _context.Monstre.Where(m => m.PointsVieBase == pvBase).ToListAsync();
        }
        // GET: api/DefenseBase/49
        [HttpGet("DefenseBase/{defenseBase}")]
        public async Task<ActionResult<IEnumerable<Monstre>>> GetMonstreByDefenseBase(int defenseBase)
        {
            return await _context.Monstre.Where(m => m.DefenseBase == defenseBase).ToListAsync();
        }

        // GET: api/XPBase/64
        [HttpGet("XPBase/{xpBase}")]
        public async Task<ActionResult<IEnumerable<Monstre>>> GetMonstreByXPBase(int xpBase)
        {
            return await _context.Monstre.Where(m => m.ExperienceBase == xpBase).ToListAsync();
        }

        // GET: api/Type/grass
        [HttpGet("Type/{type}")]
        public async Task<ActionResult<IEnumerable<Monstre>>> GetMonstreByType(string type)
        {
            if (type == null)
            {
                return NotFound();
            }

            var fullList = await _context.Monstre.ToListAsync();
            List<Monstre> monstres = new();
            foreach (Monstre monstre in fullList)
            {
                if (monstre.Type1 == type) monstres.Add(monstre);
                if (monstre.Type2 == type) monstres.Add(monstre);
            }
            return monstres;
        }
    }
}
