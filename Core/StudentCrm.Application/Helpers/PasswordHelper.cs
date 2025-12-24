using System.Security.Cryptography;

namespace StudentCrm.Application.Helpers
{
    public static class PasswordHelper
    {
        public static string GenerateTempPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%";
            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);

            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = chars[bytes[i] % chars.Length];

            return new string(result);
        }
    }
}
