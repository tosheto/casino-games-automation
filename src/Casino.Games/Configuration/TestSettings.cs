using System;

namespace Casino.Games.Configuration
{
    public static class TestSettings
    {
        // Spinberry products page
        public static string ProductsUrl =>
            Environment.GetEnvironmentVariable("SPINBERRY_PRODUCTS_URL")
            ?? "https://www.spinberry.com/#products";

        // Backwards compatibility – текущият IrishWildsPage ползва BaseUrl
        public static string BaseUrl => ProductsUrl;

        // Fallback директен линк към демо (ако popup-а не се хване)
        public static string GameUrl =>
            Environment.GetEnvironmentVariable("IRISH_WILDS_GAME_URL")
            ?? "https://tinyurl.com/39hjjjf3";

        // Колко да чакаме spin да приключи преди да се откажем
        public static int SpinWaitTimeoutMs =>
            int.TryParse(Environment.GetEnvironmentVariable("IRISH_WILDS_SPIN_TIMEOUT_MS"), out var value)
                ? value
                : 15000;

        public static class Selectors
        {
            // Ще търсим по различни семпли шаблони – важен е real game iframe-а
            public const string BalanceLabelCss =
                "[data-testid=\"balance\"], [class*=\"balance\" i], [class*=\"credit\" i]";

            public const string StakeLabelCss =
                "[data-testid=\"bet\"], [class*=\"bet\" i], [class*=\"stake\" i]";

            public const string LastWinLabelCss =
                "[data-testid=\"last-win\"], [class*=\"win\" i]";

            public const string SpinButtonCss =
                "[data-testid=\"spin-button\"], [class*=\"spin\" i], button[aria-label*=\"spin\" i]";
        }
    }
}
