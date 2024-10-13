using Infinity.Toolkit.FeatureModules;
using Slacker.Api.Shared;
using System.Text;
using System.Text.Json;

namespace Slacker.Api.Features.Slack;

internal static class SlackHttpClientExtensions
{
    public static IHttpClientBuilder AddSlackHttpClient(this ModuleContext context)
    {
        context.Services.AddTransient<SlackOAuthHttpMessageHandler>();

        return context.Services.AddHttpClient<ISlackChatHttpClient, PostMessageToChannelHttpClient>(client =>
        {
            client.BaseAddress = new("https://slack");
        })
        .AddHttpMessageHandler<SlackOAuthHttpMessageHandler>();
    }
}

public interface ISlackChatHttpClient
{
    Task<Result<SlackMessageResponse>> PostMessageToChannelAsync(string message, string channel);

    Task<Result<SlackMessageResponse>> UpdateMessageAsync(string message, string channel, string ts);

    Task<Result<SlackMessageResponse>> OpenViewAsync(string triggerId, string view);
}


internal class PostMessageToChannelHttpClient(HttpClient httpClient) : ISlackChatHttpClient
{
    private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public static JsonSerializerOptions JsonSerializerOptions { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public async Task<Result<SlackMessageResponse>> OpenViewAsync(string triggerId, string view)
    {
        var requestUri = "/views.open";

        var body = new Dictionary<string, string?>
        {
            { "trigger_id", triggerId },
            { "view", view }
        };

        using var jsonContent = JsonContent.Create(body);
        var response = await httpClient.PostAsync(requestUri, jsonContent);
        var content = await response.Content.ReadAsStringAsync();

        var slackResponse = JsonSerializer.Deserialize<SlackMessageResponse>(content, JsonSerializerOptions);

        return slackResponse?.Ok switch
        {
            true => Result.Success(slackResponse),
            _ => Result.Failure(slackResponse!)
        };
    }

    public async Task<Result<SlackMessageResponse>> PostMessageToChannelAsync(string message, string channel)
    {
        var requestUri = "/chat.postMessage";

        using StringContent jsonContent = new(message, Encoding.UTF8, "application/json");

        var result = await httpClient.PostAsync(requestUri, jsonContent);

        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<SlackMessageResponse>(JsonSerializerOptions);

        return response?.Ok switch
        {
            true => Result.Success(response),
            _ => Result.Failure(response!)
        };
    }

    public async Task<Result<SlackMessageResponse>> UpdateMessageAsync(string message, string channel, string ts)
    {
        var requestUri = "/chat.update";

        using StringContent jsonContent = new(message, Encoding.UTF8, "application/json");

        var result = await httpClient.PostAsync(requestUri, jsonContent);

        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<SlackMessageResponse>(JsonSerializerOptions);

        return response?.Ok switch
        {
            true => Result.Success(response),
            _ => Result.Failure(response!)
        };
    }
}
