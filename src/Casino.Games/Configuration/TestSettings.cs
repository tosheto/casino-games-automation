using System;
using System.IO;

namespace Casino.Games.Configuration
{
    public static class TestSettings
    {
        // Глобални настройки
        public static bool Headless => false;

        // Spinberry products страница
        public static string BaseUrl => "https://www.spinberry.com/#products";

        // Пряк demo URL за Irish Wilds (fallback)
        public static string GameUrl => "https://tinyurl.com/39hjjjf3";

        // Timeout за изчакване на промяна в баланса след спин
        public static int SpinWaitTimeoutMs => 15000;

        // Селектори, които вече ползва IrishWildsPage
        public static class Selectors
        {
            public const string BalanceLabelCss =
                "[class*=\"balance\" i], [id*=\"balance\" i], [class*=\"credit\" i]";

            public const string StakeLabelCss =
                "[class*=\"bet\" i], [class*=\"stake\" i]";

            public const string LastWinLabelCss =
                "[class*=\"win\" i]";

            public const string SpinButtonCss =
                "button[aria-label*=\"spin\" i], button[class*=\"spin\" i], [class*=\"spin-button\" i]";
        }

        // Помощен метод - гарантира, че папката съществува
        private static string Ensure(string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }

        // Root за артефакти (screenshots, html, videos)
        public static string ArtifactsRoot =>
            Ensure(Path.Combine(AppContext.BaseDirectory, "artifacts"));

        public static string ScreenshotsDir =>
            Ensure(Path.Combine(ArtifactsRoot, "screenshots"));

        public static string HtmlDumpDir =>
            Ensure(Path.Combine(ArtifactsRoot, "html"));

        public static string VideoOutputDir =>
            Ensure(Path.Combine(ArtifactsRoot, "videos"));
    }
}
