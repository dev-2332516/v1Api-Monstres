namespace ApiV1ControlleurMonstre.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; }       // Clé secrète pour signer le token
        public string Issuer { get; set; }       // Émetteur du token
        public string Audience { get; set; }     // Destinataire du token
        public int TokenLifetimeMinutes { get; set; }  // Durée de validité en minutes
    }
}
