using System.Net.Http.Json;
using Dreamstreaming.SeerrDiscord.Models;

namespace Dreamstreaming.SeerrDiscord.Services;

public class DiscordWebhookService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DiscordWebhookService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendMessageAsync(
        string webhookUrl,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return;
        }

        var client =
            _httpClientFactory.CreateClient();

        var payload = new
        {
            content = message,
            allowed_mentions = CreateAllowedMentions(message)
        };

        using var response =
            await client.PostAsJsonAsync(
                webhookUrl,
                payload,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task SendSeerrMessageAsync(
        string webhookUrl,
        string title,
        string message,
        SeerrWebhookPayload seerrPayload,
        bool useEmbed,
        bool showPoster,
        bool showMediaType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            return;
        }

        if (!useEmbed)
        {
            var plainMessage =
                BuildPlainMessage(
                    title,
                    message,
                    seerrPayload,
                    showMediaType);

            await SendMessageAsync(
                webhookUrl,
                plainMessage,
                cancellationToken);

            return;
        }

        await SendEmbedAsync(
            webhookUrl,
            title,
            message,
            seerrPayload,
            showPoster,
            showMediaType,
            cancellationToken);
    }

    private async Task SendEmbedAsync(
        string webhookUrl,
        string title,
        string message,
        SeerrWebhookPayload seerrPayload,
        bool showPoster,
        bool showMediaType,
        CancellationToken cancellationToken)
    {
        var client =
            _httpClientFactory.CreateClient();

        var fields =
            new List<object>();

        if (showMediaType &&
            !string.IsNullOrWhiteSpace(
                seerrPayload.MediaType))
        {
            fields.Add(
                new
                {
                    name = "Media type",
                    value = FormatMediaType(
                        seerrPayload.MediaType),
                    inline = true
                });
        }

        if (!string.IsNullOrWhiteSpace(
                seerrPayload.RequestedBy))
        {
            fields.Add(
                new
                {
                    name = "Aangevraagd door",
                    value = seerrPayload.RequestedBy,
                    inline = true
                });
        }

        if (!string.IsNullOrWhiteSpace(
                seerrPayload.RequestId))
        {
            fields.Add(
                new
                {
                    name = "Request ID",
                    value = seerrPayload.RequestId,
                    inline = true
                });
        }

        object? thumbnail = null;

        if (showPoster &&
            IsValidHttpUrl(
                seerrPayload.Image))
        {
            thumbnail = new
            {
                url = seerrPayload.Image
            };
        }

        var embed = new
        {
            title,
            description = message,
            fields = fields.ToArray(),
            thumbnail,
            timestamp =
                DateTime.UtcNow
                    .ToString("O")
        };

        var discordPayload = new
        {
            content = (string?)null,
            embeds = new[]
            {
                embed
            },
            allowed_mentions =
                CreateAllowedMentions(message)
        };

        using var response =
            await client.PostAsJsonAsync(
                webhookUrl,
                discordPayload,
                cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private static string BuildPlainMessage(
        string title,
        string message,
        SeerrWebhookPayload seerrPayload,
        bool showMediaType)
    {
        var result =
            $"{title}\n\n{message}";

        if (showMediaType &&
            !string.IsNullOrWhiteSpace(
                seerrPayload.MediaType))
        {
            result +=
                $"\n\nType: " +
                $"{FormatMediaType(seerrPayload.MediaType)}";
        }

        return result;
    }

    private static object CreateAllowedMentions(
        string message)
    {
        return new
        {
            parse =
                Array.Empty<string>(),

            users =
                ExtractDiscordUserIds(
                    message)
        };
    }

    private static string[] ExtractDiscordUserIds(
        string message)
    {
        var ids =
            new List<string>();

        var startIndex = 0;

        while ((startIndex =
                    message.IndexOf(
                        "<@",
                        startIndex,
                        StringComparison.Ordinal)) >= 0)
        {
            var endIndex =
                message.IndexOf(
                    '>',
                    startIndex);

            if (endIndex < 0)
            {
                break;
            }

            var value =
                message.Substring(
                    startIndex + 2,
                    endIndex - startIndex - 2);

            value =
                value.TrimStart('!');

            if (ulong.TryParse(
                    value,
                    out _))
            {
                ids.Add(value);
            }

            startIndex =
                endIndex + 1;
        }

        return ids
            .Distinct()
            .ToArray();
    }

    private static bool IsValidHttpUrl(
        string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (!Uri.TryCreate(
                url,
                UriKind.Absolute,
                out var uri))
        {
            return false;
        }

        return uri.Scheme ==
                   Uri.UriSchemeHttp ||
               uri.Scheme ==
                   Uri.UriSchemeHttps;
    }

    private static string FormatMediaType(
        string mediaType)
    {
        if (string.IsNullOrWhiteSpace(
                mediaType))
        {
            return "Onbekend";
        }

        return mediaType
            .Trim()
            .ToLowerInvariant() switch
        {
            "movie" => "Film",
            "tv" => "Serie",
            "series" => "Serie",
            "season" => "Seizoen",
            "episode" => "Aflevering",
            _ => mediaType
        };
    }
}