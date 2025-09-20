using System.Security.Cryptography;
using System.Text;

namespace ApiV1ControlleurMonstre
{
    public static class Hashing
    {
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

        // Trouve sur https://gist.github.com/enif-lee/c2c38c53d8cd2febb2f14922153352c0
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly Random Random = new Random();

        public static string GenerateToken(int length)
        {
            return GenerateToken(Alphabet, length);
        }

        public static string GenerateToken(string characters, int length)
        {
            return new string(Enumerable
              .Range(0, length)
              .Select(num => characters[Random.Next() % characters.Length])
              .ToArray());
        }

        public static string GenerateHash(string text)
        {
            using (var hash = SHA256.Create())
            {
                return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
            }
        }

        public static bool CompareHash(string hash, string text)
        {
            return GenerateHash(text) == hash;
        }

    }
}
