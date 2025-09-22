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
        public bool IsConnected { get; set; }
        public Utilisateur() { }
        public Utilisateur(string email, string pseudo, string password)
        {
            Email = email;
            Pseudo = pseudo;
            Password = password;
        }
    }
}
