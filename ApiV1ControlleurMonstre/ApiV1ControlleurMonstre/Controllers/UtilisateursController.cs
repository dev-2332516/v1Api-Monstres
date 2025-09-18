using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiV1ControlleurMonstre.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateursController : Controller
    {
        private readonly MonsterContext _context;

        public UtilisateursController(MonsterContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<Utilisateur>> Register([FromBody] Utilisateur utilisateur)
        {
            utilisateur.DateInscription = DateTime.Now;
            // Immediatement hasher le mot de passe
            utilisateur.Password = Hashing.Compute(utilisateur.Password);              

            // Éviter un Id redondant
            utilisateur.Id = 0;
            // Prendre une liste complète pour crée l'Id
            int lastId = _context.Utilisateurs.ToListAsync().Result.Last().Id;
            if (lastId == 0) utilisateur.Id = 0;
            else utilisateur.Id = lastId + 1;

            if (ModelState.IsValid && !UtilisateurExist(utilisateur.Email))
            {
                _context.Add(utilisateur);
                await _context.SaveChangesAsync();
            }
            else if (!ModelState.IsValid) return BadRequest("Invalid: JSON entry was invalid");
            else if (UtilisateurExist(utilisateur.Email)) return Conflict("EmailAlreadyExists: Email adress entered already exists");

            return utilisateur;
        }

        //// GET: Utilisateurs
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.Utilisateurs.ToListAsync());
        //}

        // POST: Utilisateurs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Email,Pseudo,Password,DateInscription")] Utilisateur utilisateur)
        //{
        //    if (id != utilisateur.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(utilisateur);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UtilisateurExists(utilisateur.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(utilisateur);
        //}

        // GET: Utilisateurs/Delete/5

        private bool UtilisateurExist(string email)
        {
            return _context.Utilisateurs.Any(user => user.Email == email);
        }

        // Code tirée de l'article suivant https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        public static bool IsValidEmailOther(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            try
            {
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

                string DomainMapper(Match match)
                {
                    var idn = new IdnMapping();
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e) { return false; } 
            catch (ArgumentException e) { return false; }
           
            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException) { return false; }
        }
    }
}
