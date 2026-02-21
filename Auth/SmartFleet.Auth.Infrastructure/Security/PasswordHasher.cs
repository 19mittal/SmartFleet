using SmartFleet.Auth.Application.Interfaces;

namespace SmartFleet.Auth.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        // WorkFactor 12 is a good balance between security and performance (approx 250ms per hash)
        private const int WorkFactor = 12;

        public string HashPassword(string password)
        {
            // BCrypt generates a salt and prepends it to the hash automatically
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
