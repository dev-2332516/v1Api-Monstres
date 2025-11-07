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
    public class ServiceTimestampsController : Controller
    {
        private readonly MonsterContext _context;

        public ServiceTimestampsController(MonsterContext context)
        {
            _context = context;
        }

        // POST: ServiceTimestamps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpGet("GetTimestamp")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult<long>> GetTimestamp()
        {
            try
            {
                return _context.ServiceTimestamps.ToListAsync().Result.Last().Timestamp;
            }
            catch (Exception ex)
            {
                return BadRequest("Could not fetch timestamps");
            }
        }
    }
}
