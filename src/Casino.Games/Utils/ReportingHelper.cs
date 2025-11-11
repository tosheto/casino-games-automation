using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Casino.Games.Utils
{
    public static class ReportingHelper
    {
        private static string EnsureDir(string sub)
        {
            var root = AppContext.BaseDirectory;
            var dir = Path.Combine(root, "artifacts", sub);
            Directory.CreateDirectory(dir);
            return dir;
        }

        private static string SafeName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        public static async Task<string> CaptureScreenshotAsync(IPage page, string name)
        {
            try
            {
                var dir = EnsureDir("screenshots");
                var fileName = $"{SafeName(name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
                var fullPath = Path.Combine(dir, fileName);

                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = fullPath,
                    FullPage = true
                });

                TestContext.WriteLine($"[Reporting] Screenshot saved: {fullPath}");
                TestContext.AddTestAttachment(fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Reporting] Screenshot failed: {ex}");
                return string.Empty;
            }
        }

        public static async Task<string> DumpHtmlAsync(IPage page, string name)
        {
            try
            {
                var dir = EnsureDir("html");
                var fileName = $"{SafeName(name)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
                var fullPath = Path.Combine(dir, fileName);

                var html = await page.ContentAsync();
                File.WriteAllText(fullPath, html);

                TestContext.WriteLine($"[Reporting] HTML dump saved: {fullPath}");
                TestContext.AddTestAttachment(fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Reporting] HTML dump failed: {ex}");
                return string.Empty;
            }
        }

        public static async Task AttachVideosAsync(IBrowserContext context)
        {
            try
            {
                var dir = EnsureDir("videos");

                foreach (var page in context.Pages)
                {
                    var video = page.Video;
                    if (video == null) continue;

                    var path = await video.PathAsync();
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        continue;

                    var fileName = $"{SafeName(page.Url)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.webm";
                    var dest = Path.Combine(dir, fileName);
                    File.Copy(path, dest, true);

                    TestContext.WriteLine($"[Reporting] Video saved: {dest}");
                    TestContext.AddTestAttachment(dest);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"[Reporting] AttachVideos failed: {ex}");
            }
        }
    }
}
