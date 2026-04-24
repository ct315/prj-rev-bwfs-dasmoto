using PuppeteerSharp;

namespace NotebookLmSender.Host;

internal sealed class NotebookLmAutomation
{
    private readonly HostLogger _logger;

    public NotebookLmAutomation(HostLogger logger)
    {
        _logger = logger;
    }

    public async Task<List<NotebookEntry>> ListNotebooksAsync(IBrowser browser)
    {
        var page = await browser.NewPageAsync();
        await page.GoToAsync("https://notebooklm.google.com", WaitUntilNavigation.Networkidle0);
        await EnsureSessionAsync(page);

        var selector = "a[href*='/notebook/']";
        await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 15000 });

        var items = await page.EvaluateExpressionAsync<List<NotebookEntry>>(@"Array.from(document.querySelectorAll('a[href*=""/notebook/""]')).map(a => ({ name: (a.textContent || '').trim(), id: a.getAttribute('href') }))");
        return items.Where(i => !string.IsNullOrWhiteSpace(i.Name)).DistinctBy(i => i.Name).ToList();
    }

    public async Task EnsureNotebookExistsAsync(IBrowser browser, string notebookName)
    {
        var notebooks = await ListNotebooksAsync(browser);
        if (!notebooks.Any(n => string.Equals(n.Name, notebookName, StringComparison.OrdinalIgnoreCase)))
        {
            throw HostException.NotFound("Could not find selected notebook", ErrorCodes.NotebookNotFound);
        }
    }

    public async Task CreateNotebookAsync(IBrowser browser, string notebookName)
    {
        var page = await browser.NewPageAsync();
        await page.GoToAsync("https://notebooklm.google.com", WaitUntilNavigation.Networkidle0);
        await EnsureSessionAsync(page);

        var createButtonSelectors = new[]
        {
            "button[aria-label*='Create']",
            "button:has-text('Create')"
        };

        if (!await ClickWithFallbackAsync(page, createButtonSelectors))
        {
            throw HostException.UiChanged();
        }

        await page.Keyboard.TypeAsync(notebookName);
        await page.Keyboard.PressAsync("Enter");
        await Task.Delay(1000);

        if (await DetectNotebookLimitAsync(page))
        {
            throw HostException.WithCode("Notebook limit reached", ErrorCodes.NotebookLimitReached);
        }
    }

    public async Task AddSourceUrlAsync(IBrowser browser, string notebookName, string url)
    {
        var page = await browser.NewPageAsync();
        await page.GoToAsync("https://notebooklm.google.com", WaitUntilNavigation.Networkidle0);
        await EnsureSessionAsync(page);

        var notebookLink = await page.XPathAsync($"//a[contains(normalize-space(.), \"{EscapeForXpath(notebookName)}\")]");
        if (notebookLink.Length == 0)
        {
            throw HostException.NotFound("Could not find selected notebook", ErrorCodes.NotebookNotFound);
        }

        await notebookLink[0].ClickAsync();
        await page.WaitForNavigationAsync(new NavigationOptions { WaitUntil = [WaitUntilNavigation.Networkidle0] });

        var addSourceSelectors = new[]
        {
            "button[aria-label*='Add source']",
            "button:has-text('Add source')"
        };

        if (!await ClickWithFallbackAsync(page, addSourceSelectors))
        {
            throw HostException.UiChanged();
        }

        if (!await ClickWithFallbackAsync(page, new[] { "button:has-text('Web URL')", "button[aria-label*='Web URL']" }))
        {
            throw HostException.UiChanged();
        }

        await page.Keyboard.TypeAsync(url);
        await page.Keyboard.PressAsync("Enter");
        await Task.Delay(1500);

        if (await DetectSourceLimitAsync(page))
        {
            throw HostException.WithCode("Source limit reached", ErrorCodes.SourceLimitReached);
        }

        if (await HasTextAsync(page, "Sign in") || await HasTextAsync(page, "session expired"))
        {
            throw HostException.WithCode("Google session expired", ErrorCodes.SessionExpired);
        }
    }

    private async Task EnsureSessionAsync(IPage page)
    {
        if (await HasTextAsync(page, "Sign in") && page.Url.Contains("accounts.google.com", StringComparison.OrdinalIgnoreCase))
        {
            throw HostException.WithCode("Google session expired", ErrorCodes.SessionExpired);
        }
    }

    private async Task<bool> ClickWithFallbackAsync(IPage page, IEnumerable<string> selectors)
    {
        foreach (var selector in selectors)
        {
            try
            {
                _logger.Debug($"Trying selector: {selector}");
                await page.WaitForSelectorAsync(selector, new WaitForSelectorOptions { Timeout = 3000 });
                await page.ClickAsync(selector);
                return true;
            }
            catch
            {
                // keep trying fallback selectors
            }
        }

        return false;
    }

    private static async Task<bool> HasTextAsync(IPage page, string text)
    {
        var result = await page.EvaluateExpressionAsync<bool>($"document.body && document.body.innerText.toLowerCase().includes('{text.ToLowerInvariant()}')");
        return result;
    }

    private static async Task<bool> DetectNotebookLimitAsync(IPage page) =>
        await HasTextAsync(page, "100 notebooks") || await HasTextAsync(page, "notebook limit");

    private static async Task<bool> DetectSourceLimitAsync(IPage page) =>
        await HasTextAsync(page, "50 sources") || await HasTextAsync(page, "source limit");

    private static string EscapeForXpath(string input) => input.Replace("\"", "");
}
