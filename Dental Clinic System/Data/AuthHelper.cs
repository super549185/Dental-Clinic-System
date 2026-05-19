using System;
using System.Security.Cryptography;
using System.Text;

namespace Dental_Clinic_System.Data
{
    public static class AuthHelper
    {
        /// <summary>
        /// Hash a password using SHA256
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify a password against its hash
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            try
            {
                var hashOfInput = HashPassword(password);
                return hashOfInput == hash;
            }
            catch
            {
                return false;
            }
        }
    }
}
