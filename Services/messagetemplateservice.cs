using Dreamstreaming.SeerrDiscord.Models;

namespace Dreamstreaming.SeerrDiscord.Services;

public class MessageTemplateService
{
    public string Format(
        string template,
        SeerrWebhookPayload payload,
        string discordUserId = "")
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        var requestedBy =
            string.IsNullOrWhiteSpace(payload.RequestedBy)
                ? "Unknown user"
                : payload.RequestedBy.Trim();

        var cleanDiscordUserId =
            discordUserId?.Trim()
            ?? string.Empty;

        var mention =
            string.IsNullOrWhiteSpace(cleanDiscordUserId)
                ? requestedBy
                : $"<@{cleanDiscordUserId}>";

        var title =
            payload.GetDisplayTitle();

        var mediaType =
            payload.MediaType?.Trim()
            ?? string.Empty;

        var requestId =
            payload.RequestId?.Trim()
            ?? string.Empty;

        var jellyfinUserId =
            payload.JellyfinUserId?.Trim()
            ?? string.Empty;

        var mediaStatus =
            payload.MediaStatus?.Trim()
            ?? string.Empty;

        var jellyfinMediaId =
            payload.JellyfinMediaId?.Trim()
            ?? string.Empty;

        return template
            .Replace(
                "{user}",
                requestedBy,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mention}",
                mention,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{discordUserId}",
                cleanDiscordUserId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{title}",
                title,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mediaType}",
                mediaType,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{requestId}",
                requestId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{jellyfinUserId}",
                jellyfinUserId,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{mediaStatus}",
                mediaStatus,
                StringComparison.OrdinalIgnoreCase)
            .Replace(
                "{jellyfinMediaId}",
                jellyfinMediaId,
                StringComparison.OrdinalIgnoreCase);
    }
}