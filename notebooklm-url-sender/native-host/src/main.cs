using NotebookLmSender.Host;
using PuppeteerSharp;

try
{
    var request = HostProtocol.ReadRequest(Console.OpenStandardInput());
    var logger = new HostLogger(request.DebugMode ?? false);
    var cache = new NotebookCache();
    var automation = new NotebookLmAutomation(logger);

    logger.Info($"Action={request.Action}");

    object response;

    if (request.Action == "list_notebooks")
    {
        await using var browser = await ChromeLaunch.LaunchAsync(logger);
        var notebooks = await automation.ListNotebooksAsync(browser);
        cache.Write(notebooks);
        response = new
        {
            ok = true,
            action = request.Action,
            notebooks
        };
    }
    else if (request.Action is "add_sources" or "create_notebook_and_add_sources")
    {
        var urls = (request.Urls ?? [])
            .Where(u => Uri.TryCreate(u, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .Distinct()
            .ToList();

        if (urls.Count == 0)
        {
            response = Fail(request.Action, ErrorCodes.InvalidRequest, "Could not add URL source");
            HostProtocol.WriteResponse(Console.OpenStandardOutput(), response);
            return;
        }

        await using var browser = await ChromeLaunch.LaunchAsync(logger);

        var targetNotebook = request.TargetNotebook;
        var createdNotebook = false;
        if (request.Action == "create_notebook_and_add_sources")
        {
            if (string.IsNullOrWhiteSpace(request.NewNotebookName))
            {
                response = Fail(request.Action, ErrorCodes.InvalidRequest, "Notebook creation cancelled");
                HostProtocol.WriteResponse(Console.OpenStandardOutput(), response);
                return;
            }

            targetNotebook = request.NewNotebookName.Trim();
            await automation.CreateNotebookAsync(browser, targetNotebook);
            createdNotebook = true;
        }

        if (string.IsNullOrWhiteSpace(targetNotebook))
        {
            response = Fail(request.Action, ErrorCodes.NotebookNotFound, "Could not find selected notebook");
            HostProtocol.WriteResponse(Console.OpenStandardOutput(), response);
            return;
        }

        await automation.EnsureNotebookExistsAsync(browser, targetNotebook);

        var added = new List<string>();
        var failed = new List<UrlFailure>();

        foreach (var url in urls)
        {
            try
            {
                await automation.AddSourceUrlAsync(browser, targetNotebook, url);
                added.Add(url);
            }
            catch (HostException ex) when (ex.ErrorCode is ErrorCodes.SourceLimitReached or ErrorCodes.NotebookLimitReached)
            {
                response = Fail(request.Action, ex.ErrorCode, ex.Message);
                HostProtocol.WriteResponse(Console.OpenStandardOutput(), response);
                return;
            }
            catch (HostException ex)
            {
                failed.Add(new UrlFailure(url, ex.Message));
            }
            catch (Exception ex)
            {
                logger.Error($"URL add failed for {url}: {ex.Message}");
                failed.Add(new UrlFailure(url, "Could not add URL source"));
            }
        }

        response = new
        {
            ok = failed.Count == 0,
            action = request.Action,
            targetNotebook,
            createdNotebook,
            added,
            failed
        };
    }
    else
    {
        response = Fail(request.Action, ErrorCodes.InvalidRequest, "Invalid action");
    }

    HostProtocol.WriteResponse(Console.OpenStandardOutput(), response);
}
catch (HostException ex)
{
    HostProtocol.WriteResponse(Console.OpenStandardOutput(), Fail("unknown", ex.ErrorCode, ex.Message));
}
catch (Exception ex)
{
    HostProtocol.WriteResponse(Console.OpenStandardOutput(), Fail("unknown", ErrorCodes.Unknown, ex.Message));
}

static object Fail(string action, string code, string message) => new
{
    ok = false,
    action,
    errorCode = code,
    message
};

namespace NotebookLmSender.Host
{
    internal sealed class HostException : Exception
    {
        public string ErrorCode { get; }

        private HostException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }

        public static HostException WithCode(string message, string code) => new(message, code);
        public static HostException NotFound(string message, string code) => new(message, code);
        public static HostException UiChanged() => new("NotebookLM page changed, automation needs update", ErrorCodes.AutomationUiChanged);
    }
}
