using System;
using System.Linq;

namespace ApplicationCore.Utils
{
    public static class Utils
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static string GenerateRandomId(int length)
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            return new string(Enumerable.Repeat(Chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
