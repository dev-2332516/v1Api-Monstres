using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiV1ControlleurMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaughtMonsterController : ControllerBase
    {
        private readonly MonsterContext _context;

        public CaughtMonsterController(MonsterContext context)
        {
            _context = context;
        }

        // GET: api/<CaughtMonsterController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<CaughtMonsterController>/
        [HttpGet("GetFromUser/{id}")]
        public async Task<IActionResult> GetMonstreFromUser(int id)
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
                var caughtMonsters = await _context.CaughtMonsters.Where(x => x.whoHasCaught == personnage).ToListAsync();
                return (IActionResult)caughtMonsters;
            }
            return null;
        }

        // POST api/<CaughtMonsterController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CaughtMonsterController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
    }
}
