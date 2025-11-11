using System;
using System.Globalization;
using System.Text;
using NUnit.Framework;

namespace Casino.Games.Utils
{
    public static class BalanceHelper
    {
        private static readonly CultureInfo[] CandidateCultures =
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("en-US"),
            new CultureInfo("en-GB")
        };

        public static decimal ParseAmount(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException("Amount text is null or empty.");

            var clean = raw
                .Replace("Balance", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Win", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Bet", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Stake", "", StringComparison.OrdinalIgnoreCase)
                .Replace("Credits", "", StringComparison.OrdinalIgnoreCase)
                .Replace(":", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();

            var sb = new StringBuilder();
            foreach (var ch in clean)
            {
                if (char.IsDigit(ch) || ch == '.' || ch == ',' || ch == '-')
                    sb.Append(ch);
            }

            var numeric = sb.ToString();

            foreach (var culture in CandidateCultures)
            {
                if (decimal.TryParse(numeric, NumberStyles.Any, culture, out var value))
                    return value;
            }

            throw new FormatException($"Unable to parse amount from '{raw}'.");
        }

        public static void AssertBalance(decimal expected, decimal actual, string description, decimal tolerance = 0.01m)
        {
            var delta = Math.Abs(expected - actual);
            if (delta > tolerance)
            {
                Assert.Fail($"{description} - Expected: {expected}, Actual: {actual}, Delta: {delta} (> {tolerance})");
            }
        }
    }
}
