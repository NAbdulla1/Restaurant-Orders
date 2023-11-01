using System.Security.Cryptography;

namespace Restaurant_Orders.Services
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string password);
    }

    public class PasswordService : IPasswordService
    {
        // do not change these values if some passwords are already saved in the database
        private const int Iterations = 10000;
        private const int SaltSize = 16;
        private const int Pbkdf2Bytes = 32;

        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(Pbkdf2Bytes);

            byte[] hashBytes = new byte[SaltSize + Pbkdf2Bytes];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, Pbkdf2Bytes);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string hashedPassword, string password)
        {
            //Retrieve saved password salt
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            //Retrieve saved password hash
            byte[] savedHash = new byte[Pbkdf2Bytes];
            Array.Copy(hashBytes, SaltSize, savedHash, 0, Pbkdf2Bytes);

            //hash the password using the salt to reproduct the hash.
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
            byte[] reproducedHash = pbkdf2.GetBytes(Pbkdf2Bytes);

            for (int byteIndex = 0; byteIndex < Pbkdf2Bytes; byteIndex++)
            {
                if (savedHash[byteIndex] != reproducedHash[byteIndex])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
