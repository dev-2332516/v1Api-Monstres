using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;

namespace ApiV1ControlleurMonstre.Controllers
{
    public class QuetesController : Controller
    {
        private readonly MonsterContext _context;

        public QuetesController(MonsterContext context)
        {
            _context = context;
        }

        // GET: Quetes
        [HttpGet("GetQuetes/{id}")]
        public async Task<ActionResult<List<Quete>>> Index(int id)
        {
            return await _context.Quetes.Where(quete => quete.Personnage.Id == id).ToListAsync();
        }

    }
}
