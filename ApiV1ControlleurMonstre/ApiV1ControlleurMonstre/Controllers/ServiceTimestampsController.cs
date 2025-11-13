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
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTimestampsController : Controller
    {
        private readonly MonsterContext _context;

        public ServiceTimestampsController(MonsterContext context)
        {
            _context = context;
        }

        [HttpGet("GetTimestamp/")]
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
