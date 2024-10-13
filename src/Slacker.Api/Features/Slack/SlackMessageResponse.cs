using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slacker.Api.Features.Slack;

public record SlackMessageResponse
{
    public bool Ok { get; init; }

    public string Channel { get; init; }

    [JsonPropertyName("ts")]
    public string Timestamp { get; init; }

    public JsonElement Message { get; init; }

    public string Error { get; init; }

    public string[] Errors { get; init; } = [];
}
