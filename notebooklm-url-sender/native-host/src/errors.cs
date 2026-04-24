namespace NotebookLmSender.Host;

internal static class ErrorCodes
{
    public const string NotebookNotFound = "NOTEBOOK_NOT_FOUND";
    public const string NotebookLimitReached = "NOTEBOOK_LIMIT_REACHED";
    public const string SourceLimitReached = "SOURCE_LIMIT_REACHED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string CouldNotAddUrl = "COULD_NOT_ADD_URL";
    public const string AutomationUiChanged = "AUTOMATION_UI_CHANGED";
    public const string InvalidRequest = "INVALID_REQUEST";
    public const string Unknown = "UNKNOWN";
}

internal sealed record UrlFailure(string Url, string Reason);
