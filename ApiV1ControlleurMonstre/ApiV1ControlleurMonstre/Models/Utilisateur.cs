using Microsoft.Build.ObjectModelRemoting;
using System.Security;

namespace ApiV1ControlleurMonstre.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Pseudo {  get; set; }
        public string Password { get; set; }
        public DateTime DateInscription { get; set; }

        public Utilisateur() { }
        public Utilisateur(int id, string email, string pseudo, DateTime dateInscription)
        {
            Id = id;
            Email = email;
            Pseudo = pseudo;
            DateInscription = DateTime.Now;
        }
    }
}
