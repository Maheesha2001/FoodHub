using System;
using System.Security.Cryptography;
using System.Text;

namespace FoodHub.Helpers
{
    public static class CodeGenerator
    {
        private static readonly object _lock = new();

        public static string GenerateFexCode()
        {
            lock (_lock) // thread-safe
            {
                string prefix = "FEX";
                string datePart = DateTime.UtcNow.ToString("yyyyMMdd");

                // Generate a secure random 5-digit number
                int randomPart = RandomNumberGenerator.GetInt32(10000, 99999);

                return $"{prefix}-{datePart}-{randomPart}";
            }
        }
    }
}
