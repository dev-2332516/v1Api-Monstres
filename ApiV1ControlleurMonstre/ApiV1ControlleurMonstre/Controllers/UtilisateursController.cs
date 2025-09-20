using ApiV1ControlleurMonstre.Data.Context;
using ApiV1ControlleurMonstre.Models;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
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
            // Immediatement hasher le mot de passe et creer un token
            utilisateur.Password = Hashing.Compute(utilisateur.Password);
            utilisateur.Token = Hashing.GenerateToken(32);

            // Éviter un Id redondant
            utilisateur.Id = 0;
            // Prendre une liste complète pour crée l'Id
            int lastId = 0;
            try
            {
                lastId = _context.Utilisateurs.ToListAsync().Result.Last().Id;
            }
            catch
            {
                if (lastId == 0) utilisateur.Id = 0;
                else utilisateur.Id = lastId + 1;
            }

            if (ModelState.IsValid && !UtilisateurExist(utilisateur.Email))
            {
                _context.Add(utilisateur);
                await _context.SaveChangesAsync();
            }
            else if (!ModelState.IsValid) return BadRequest("Invalid: JSON entry was invalid");
            else if (UtilisateurExist(utilisateur.Email)) return Conflict("EmailAlreadyExists: Email adress entered already exists");

            return utilisateur;
        }

        [HttpPost("login/{email}/{password}")]
        public async Task<ActionResult<Utilisateur>> Login(string email, string password)
        {
            string hashedPassword = Hashing.Compute(password);
            password = null;
            Utilisateur utilisateur = _context.Utilisateurs.ToListAsync().Result.Where(user => user.Email == email || password == hashedPassword).First();
            if (utilisateur is null) return BadRequest("InvalidEmailPassword: Email or password is incorrect");
            // Connecte l'utilisateur avant de le retirer
            _context.Remove(utilisateur);
            utilisateur.IsConnected = true;
            _context.Add(utilisateur);
            await _context.SaveChangesAsync();
            return utilisateur;
        }

        [HttpPost("logout/{id}")]
        public async Task<ActionResult> Logout(int id)
        {
            // Connecte l'utilisateur avant de le retirer
            Utilisateur utilisateur = _context.Utilisateurs.ToListAsync().Result.Where(user => user.Id == id).First();
            if (utilisateur == null) return BadRequest("InvalidID: Id invalide");
            else if (utilisateur.IsConnected)
            {
                _context.Remove(utilisateur);
                utilisateur.IsConnected = false;
                _context.Add(utilisateur);
                await _context.SaveChangesAsync();
            }
            return Ok();
        }


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
