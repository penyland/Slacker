using Infinity.Toolkit.FeatureModules;
using Slacker.Api.Shared;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Slacker.Api.Features.Slack;

public class SlackModule : WebFeatureModule
{
    public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public override IModuleInfo? ModuleInfo { get; } = new FeatureModuleInfo("Slack", "1.0.0");

    public override ModuleContext RegisterModule(ModuleContext context)
    {
        context.AddSlackHttpClient();

        return base.RegisterModule(context);
    }

    public override void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/slack").WithTags("Slack");

        group.MapPost("/", async (HttpRequest httpRequest, ISlackChatHttpClient httpClient) =>
        {
            var payload = httpRequest.Form.ToDictionary(split => split.Key, split => Uri.UnescapeDataString(split.Value.ToString()));
            var json = JsonSerializer.Serialize(payload);
            var slackActionPayload = JsonDocument.Parse(json).Deserialize<SlackActionPayload>();

            var result = await httpClient.OpenViewAsync(slackActionPayload!.TriggerId, modal);
            return result switch
            {
                Success => TypedResults.Json(new SlackCommandResponse { Text = "Thank you!" }),
                Failure => TypedResults.Json(new SlackCommandResponse { Text = result.Error.Details }),
                _ => TypedResults.Json(new SlackCommandResponse { Text = "Something went wrong!" })
            };
        });

        group.MapPost("/events", async (HttpRequest httpRequest, ILogger<SlackModule> logger) =>
        {
            logger?.LogInformation("Received event from Slack");
            using StreamReader reader = new(httpRequest.Body);
            var json = await reader.ReadToEndAsync();
            Console.WriteLine(json);
            return TypedResults.Json(new SlackCommandResponse { Text = "Thank you!" });
        });
    }

    private string modal = """
        {
            "type": "modal",
            "callback_id": "gratitude-modal",
            "title": {
                "type": "plain_text",
                "text": "Gratitude Box",
                "emoji": true
            },
            "submit": {
                "type": "plain_text",
                "text": "Submit",
                "emoji": true
            },
            "close": {
                "type": "plain_text",
                "text": "Cancel",
                "emoji": true
            },
            "blocks": [
                {
                    "type": "input",
                    "block_id": "my_block",
                    "element": {
                        "type": "plain_text_input",
                        "action_id": "my_action"
                    },
                    "label": {
                        "type": "plain_text",
                        "text": "Say something nice!",
                        "emoji": true
                    }
                }
            ]
        }
        """;
}

internal record SlackActionPayload
{
    [JsonPropertyName("team_id")]
    public string TeamId { get; set; }

    [JsonPropertyName("team_domain")]
    public string TeamDomain { get; set; }

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; }

    [JsonPropertyName("channel_name")]
    public string ChannelName { get; set; }

    [JsonPropertyName("user_id")]
    public string UserId { get; set; }

    [JsonPropertyName("user_name")]
    public string UserName { get; set; }

    [JsonPropertyName("command")]
    public string Command { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("response_url")]
    public string ResponseUrl { get; set; }

    [JsonPropertyName("trigger_id")]
    public string TriggerId { get; set; }

    [JsonPropertyName("api_app_id")]
    public string? ApiAppId { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }
}

internal record SlackCommandResponse
{
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = "ephemeral";

    public string Text { get; set; }
}
