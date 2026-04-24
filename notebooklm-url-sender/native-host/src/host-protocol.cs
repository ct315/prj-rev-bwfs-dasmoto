using System.Text;
using System.Text.Json;

namespace NotebookLmSender.Host;

internal sealed record HostRequest(
    string Action,
    string? TargetNotebook,
    string? NewNotebookName,
    List<string>? Urls,
    bool? DebugMode);

internal static class HostProtocol
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static HostRequest ReadRequest(Stream input)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        var lengthBytes = reader.ReadBytes(4);
        if (lengthBytes.Length < 4)
        {
            throw new EndOfStreamException("No native host message payload.");
        }

        var messageLength = BitConverter.ToInt32(lengthBytes, 0);
        var payloadBytes = reader.ReadBytes(messageLength);
        var json = Encoding.UTF8.GetString(payloadBytes);
        return JsonSerializer.Deserialize<HostRequest>(json, JsonOptions)
               ?? throw new InvalidOperationException("Invalid request payload.");
    }

    public static void WriteResponse(Stream output, object response)
    {
        var json = JsonSerializer.Serialize(response, JsonOptions);
        var payload = Encoding.UTF8.GetBytes(json);
        var length = BitConverter.GetBytes(payload.Length);
        output.Write(length, 0, length.Length);
        output.Write(payload, 0, payload.Length);
        output.Flush();
    }
}
