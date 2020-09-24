using System;
using System.Collections.Generic;

namespace App.Core
{
    public static class CoreHelper
    {
        private static readonly Random random = new Random();
        private static readonly IDictionary<int, string> randomStringCache = new Dictionary<int, string>();

        public static string GetRandomString(int length)
        {
            if (randomStringCache.TryGetValue(length, out var result))
            {
                return result;
            }

            const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

            result = string.Empty;

            for (int i = 0; i < length; i++)
            {
                result += validCharacters[random.Next(0, validCharacters.Length - 1)];
            }

            randomStringCache.Add(length, result);

            return result;
        }
    }
}