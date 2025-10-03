using System.Security.Cryptography;
using System.Text;

namespace ApiV1ControlleurMonstre.Utilities
{
    /// <summary>
    /// Utilitaire pour les opérations de hachage et génération de tokens
    /// </summary>
    public static class HashingUtility
    {
        /// <summary>
        /// Calcule le hash SHA256 d'une chaîne de caractères
        /// </summary>
        /// <param name="rawData">Les données à hacher</param>
        /// <returns>Le hash en hexadécimal</returns>
        public static string Compute(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Alphabet pour génération de tokens - Inspiré de https://gist.github.com/enif-lee/c2c38c53d8cd2febb2f14922153352c0
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Random = new Random();

        /// <summary>
        /// Génère un token aléatoire de la longueur spécifiée
        /// </summary>
        /// <param name="length">Longueur du token</param>
        /// <returns>Token généré</returns>
        public static string GenerateToken(int length)
        {
            return GenerateToken(Alphabet, length);
        }

        /// <summary>
        /// Génère un token aléatoire avec un alphabet personnalisé
        /// </summary>
        /// <param name="characters">Caractères à utiliser</param>
        /// <param name="length">Longueur du token</param>
        /// <returns>Token généré</returns>
        public static string GenerateToken(string characters, int length)
        {
            return new string(Enumerable
              .Range(0, length)
              .Select(num => characters[Random.Next() % characters.Length])
              .ToArray());
        }

        /// <summary>
        /// Génère un hash Base64 d'une chaîne de caractères
        /// </summary>
        /// <param name="text">Texte à hacher</param>
        /// <returns>Hash en Base64</returns>
        public static string GenerateHash(string text)
        {
            using (var hash = SHA256.Create())
            {
                return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }

        /// <summary>
        /// Compare un hash avec un texte
        /// </summary>
        /// <param name="hash">Hash à comparer</param>
        /// <param name="text">Texte original</param>
        /// <returns>True si le hash correspond</returns>
        public static bool CompareHash(string hash, string text)
        {
            return GenerateHash(text) == hash;
        }
    }
}