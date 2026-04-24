using PuppeteerSharp;

namespace NotebookLmSender.Host;

internal static class ChromeLaunch
{
    public static async Task<IBrowser> LaunchAsync(HostLogger logger)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe");
        if (!File.Exists(chromePath))
        {
            chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe");
        }

        if (!File.Exists(chromePath))
        {
            throw new FileNotFoundException("Chrome executable not found.");
        }

        var userDataDir = Path.Combine(localAppData, "Google", "Chrome", "User Data");

        try
        {
            return await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                ExecutablePath = chromePath,
                UserDataDir = userDataDir,
                Args = ["--profile-directory=Default", "--no-first-run", "--disable-background-mode"]
            });
        }
        catch (Exception ex)
        {
            logger.Error($"Chrome launch failed (profile may be locked): {ex.Message}");
            throw;
        }
    }
}
