namespace Slacker.Api.Features.Slack;

internal class SlackOAuthHttpMessageHandler(IConfiguration configuration) : DelegatingHandler
{
    private readonly IConfiguration configuration = configuration;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var botUserOAuthToken = configuration["SLACK_BOT_USER_OAUTH_TOKEN"] ?? throw new Exception("Missing configuration key SLACK_BOT_USER_OAUTH_TOKEN");

        request.Headers.Authorization = new("Bearer", botUserOAuthToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
