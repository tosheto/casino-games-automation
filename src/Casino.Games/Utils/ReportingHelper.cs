using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Utils
{
    public static class ReportingHelper
    {
        public static async Task<string> CaptureScreenshotAsync(IPage page, string name)
        {
            try
            {
                var fileName = $"{Sanitize(name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
                var path = Path.Combine(AppContext.BaseDirectory, fileName);

                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = path,
                    FullPage = true
                });

                TestContext.AddTestAttachment(path);
                TestContext.WriteLine($"[Reporting] Screenshot saved: {path}");
                return path;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Reporting] Screenshot failed: {ex.Message}");
                return string.Empty;
            }
        }

        public static async Task<string> DumpHtmlAsync(IPage page, string name)
        {
            try
            {
                var html = await page.ContentAsync();
                var fileName = $"{Sanitize(name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
                var path = Path.Combine(AppContext.BaseDirectory, fileName);

                File.WriteAllText(path, html);
                TestContext.AddTestAttachment(path);
                TestContext.WriteLine($"[Reporting] HTML dump saved: {path}");
                return path;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Reporting] HTML dump failed: {ex.Message}");
                return string.Empty;
            }
        }

        private static string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
